using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Events;

namespace X3.PlayableAnimator
{
    public class AnimatorController : ScriptableObject
    {
        private static List<int> m_InstanceControllers = new List<int>();

        [SerializeField] [HideInInspector] private int m_UnityInsID;
        [SerializeField] [HideInInspector] private int m_PlayableInsID;
        [SerializeField] private List<AnimatorControllerLayer> m_Layers = new List<AnimatorControllerLayer>();
        [SerializeField] private List<AnimatorControllerParameter> m_Parameters = new List<AnimatorControllerParameter>();

        private bool m_IsValid;
        private TickEvent m_OnPrevUpdateTick = new TickEvent();
        private TickEvent m_OnLateUpdateTick = new TickEvent();
        private StateNotifyEvent m_StateNotifyEvent = new StateNotifyEvent();
        private List<AnimationClip> m_AnimationClips;
        private AnimationLayerMixerPlayable m_LayerMixerPlayable;

        public Animator unityAnimator { get; private set; }
        public PlayableAnimator playableAnimator { get; private set; }
        public PlayableGraph playableGraph { get; private set; }
        public AnimationMixerPlayable rootMixerPlayable { get; private set; }
        public AnimationLayerMixerPlayable layerMixerPlayable => m_LayerMixerPlayable;
        public int unityInsID => m_UnityInsID;
        public int playableInsID => m_PlayableInsID;
        public TickEvent onPrevUpdateTick => m_OnPrevUpdateTick;
        public TickEvent onLateUpdateTick => m_OnLateUpdateTick;
        public StateNotifyEvent stateNotify => m_StateNotifyEvent;
        public int layersCount => m_Layers.Count;
        public int parametersCount => m_Parameters.Count;

        public List<AnimationClip> animationClips
        {
            get
            {
                if (null == m_AnimationClips)
                {
                    RecollectAnimationClips();
                }

                return m_AnimationClips;
            }
        }

        public bool isValid
        {
            get
            {
                if (!m_IsValid)
                {
                    return false;
                }

                for (var index = 0; index < m_Layers.Count; index++)
                {
                    if (!m_Layers[index].isValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static AnimatorController CreateDefault()
        {
            var ctrl = CreateInstance();
            var layer = new AnimatorControllerLayer("Base Layer", 1, AnimatorControllerLayerBlendingType.Override);
            ctrl.AddLayer(layer);
            return ctrl;
        }

        public static AnimatorController CreateInstance(List<AnimatorControllerLayer> layers = null, List<AnimatorControllerParameter> parameters = null)
        {
            var ctrl = ScriptableObject.CreateInstance<AnimatorController>();
            ctrl.AddLayer(layers);
            ctrl.AddParameter(parameters);
            m_InstanceControllers.Add(ctrl.GetInstanceID());
            return ctrl;
        }

        public static AnimatorController CopyInstance(AnimatorController ctrl)
        {
            if (null == ctrl)
            {
                return null;
            }

            ctrl = Instantiate(ctrl);
            m_InstanceControllers.Add(ctrl.GetInstanceID());
            return ctrl;
        }

        public static void DestroyInstance(AnimatorController ctrl)
        {
            if (null == ctrl)
            {
                return;
            }

            m_InstanceControllers.Remove(ctrl.GetInstanceID());
            Destroy(ctrl);
        }

        public static bool ContainInstance(AnimatorController ctrl)
        {
            if (null == ctrl)
            {
                return false;
            }

            bool result = m_InstanceControllers.Contains(ctrl.GetInstanceID());
#if UNITY_EDITOR
            if (!UnityEditor.EditorUtility.IsPersistent(ctrl) && !result)
            {
                result = true;
                m_InstanceControllers.Add(ctrl.GetInstanceID());
            }
#endif
            return result;
        }

        public static bool IsReachingThreshold(float value, float prevValue, float interval, float threshold, ref float dValue)
        {
            var delta = value - prevValue;
            if (value > 10 * interval)
            {
                value %= interval;
                prevValue = value - delta;
            }
            else
            {
                while (value > interval)
                {
                    value -= interval;
                    prevValue -= interval;
                }
            }

            if (prevValue < 0)
            {
                value += interval;
            }

            dValue = value - threshold;
            if (dValue > interval)
            {
                dValue -= interval;
            }

            var result = delta != 0 && ((prevValue + delta >= threshold && prevValue < threshold) || (value >= threshold && value - delta < threshold));
            return result;
        }

        public void OnStart()
        {
            for (var i = 0; i < parametersCount; i++)
            {
                m_Parameters[i].OnStart();
            }

            for (var i = 0; i < layersCount; i++)
            {
                m_Layers[i].OnStart();
            }
        }

        public void OnUpdate(float deltaTime)
        {
            if (!isValid) RebuildPlayable();
            onPrevUpdateTick?.Invoke();

            for (var i = 0; i < layersCount; i++)
            {
                m_Layers[i].OnUpdate(deltaTime);
            }

            for (var i = 0; i < parametersCount; i++)
            {
                m_Parameters[i].OnUpdate(this);
            }

            onLateUpdateTick?.Invoke();
        }

        private void OnDestroy()
        {
            if (null != playableAnimator && playableAnimator.runtimeAnimatorController == this)
            {
                playableAnimator.runtimeAnimatorController = null;
            }

            for (var i = 0; i < layersCount; i++)
            {
                m_Layers[i].OnDestroy();
            }

            for (var i = 0; i < parametersCount; i++)
            {
                m_Parameters[i].OnDestroy();
            }

            if (m_LayerMixerPlayable.IsValid())
            {
                m_LayerMixerPlayable.Destroy();
            }

            stateNotify.RemoveAllListeners();
            onPrevUpdateTick.RemoveAllListeners();
            onLateUpdateTick.RemoveAllListeners();
        }

        public void Clear()
        {
            m_Layers.Clear();
            m_Parameters.Clear();
        }

        public void SetWeight(float weight)
        {
            if (!rootMixerPlayable.IsValid())
            {
                return;
            }

            rootMixerPlayable.SetInputWeight(0, weight);
        }

        public void Play(string stateName, int layerIndex = 0, float normalizedTime = float.NegativeInfinity)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].Play(stateName, normalizedTime);
        }

        public void PlayInFixedTime(string stateName, int layerIndex = 0, float fixedTime = float.NegativeInfinity)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].PlayInFixedTime(stateName, fixedTime);
        }

        public void CrossFade(string stateName, int layerIndex = 0, float normalizedOffsetTime = 0, float dValue = 0)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].CrossFade(stateName, normalizedOffsetTime, dValue);
        }

        public void CrossFade(string stateName, float normalizedTransitionTime = 0, int layerIndex = 0, float normalizedOffsetTime = 0, float dValue = 0)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].CrossFade(stateName, normalizedOffsetTime, normalizedTransitionTime, dValue);
        }

        public void CrossFadeInFixedTime(string stateName, int layerIndex = 0, float fixedOffsetTime = 0, float dValue = 0)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].CrossFadeInFixedTime(stateName, fixedOffsetTime, dValue);
        }

        public void CrossFadeInFixedTime(string stateName, float fixedTransitionTime = 0, int layerIndex = 0, float fixedOffsetTime = 0, float dValue = 0)
        {
            m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].CrossFadeInFixedTime(stateName, fixedOffsetTime, fixedTransitionTime, dValue);
        }

        public bool HasState(string stateName, int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].HasState(stateName);
        }

        public bool AddState(string stateName, AnimationClip clip, int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].AddState(stateName, new ClipMotion(clip));
        }

        public bool AddState(string stateName, Motion motion, int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].AddState(stateName, motion);
        }

        public AnimatorStateInfo GetPreviousStateInfo(int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].GetPreviousStateInfo();
        }

        public AnimatorStateInfo GetCurrentStateInfo(int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].GetCurrentStateInfo();
        }

        public float? GetAnimatorStateLength(string stateName, int layerIndex)
        {
            return m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].GetState<StateMotion>(stateName)?.length;
        }

        public AnimationClip GetAnimatorStateClip(string stateName, int layerIndex)
        {
            return (m_Layers[Mathf.Clamp(layerIndex, 0, layersCount - 1)].GetState<StateMotion>(stateName)?.motion as ClipMotion)?.clip;
        }

        public int GetLayerIndex(string layerName)
        {
            for (var i = 0; i < layersCount; i++)
            {
                if (m_Layers[i].name == layerName)
                {
                    return i;
                }
            }

            return -1;
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            var layer = GetLayer(layerIndex);
            if (null == layer)
            {
                return;
            }

            m_LayerMixerPlayable.SetInputWeight(layer.mixerPlayable, weight);
        }

        public void SetLayerSpeed(int layerIndex, float speed)
        {
            GetLayer(layerIndex)?.SetSpeed(speed);
        }

        public void SetStateSpeed(string stateName, float speed, int layerIndex = 0)
        {
            GetLayer(layerIndex)?.SetStateSpeed(stateName, speed);
        }

        public bool GetBool(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            return null != parameter && parameter.defaultBool;
        }

        public int GetInteger(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            return parameter?.defaultInt ?? 0;
        }

        public float GetFloat(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            return parameter?.defaultFloat ?? 0;
        }

        public void SetBool(string parameterName, bool value)
        {
            var parameter = GetParameter(parameterName);
            if (null != parameter)
            {
                parameter.defaultBool = value;
            }
        }

        public void SetInteger(string parameterName, int value)
        {
            var parameter = GetParameter(parameterName);
            if (null != parameter)
            {
                parameter.defaultInt = value;
            }
        }

        public void SetFloat(string parameterName, float value)
        {
            var parameter = GetParameter(parameterName);
            if (null != parameter)
            {
                parameter.defaultFloat = value;
            }
        }

        public void Combine(AnimatorController other)
        {
            if (null == other)
            {
                return;
            }

            for (var index = 0; index < other.m_Layers.Count; index++)
            {
                AddLayer(other.m_Layers[index]);
            }

            for (var index = 0; index < other.m_Parameters.Count; index++)
            {
                AddParameter(other.m_Parameters[index]);
            }
        }

        public void AddLayer(List<AnimatorControllerLayer> layers)
        {
            if (null == layers)
            {
                return;
            }

            for (var index = 0; index < layers.Count; index++)
            {
                AddLayer(layers[index]);
            }
        }

        public void AddLayer(AnimatorControllerLayer layer)
        {
            if (null == layer || string.IsNullOrEmpty(layer.name) || null != GetLayer(layer.name))
            {
                return;
            }

            layer.Reset();
            m_Layers.Add(layer);
        }

        public void RemoveLayer(string name)
        {
            var layer = GetLayer(name);
            if (null == layer)
            {
                return;
            }

            m_Layers.Remove(layer);
        }

        public AnimatorControllerLayer GetLayer(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (var index = 0; index < m_Layers.Count; index++)
            {
                var layer = m_Layers[index];
                if (layer.name == name)
                {
                    return layer;
                }
            }

            return null;
        }

        public AnimatorControllerLayer GetLayer(int index)
        {
            if (index < 0 || index >= layersCount)
            {
                return null;
            }

            return m_Layers[index];
        }

        public void AddParameter(List<AnimatorControllerParameter> parameters)
        {
            if (null == parameters)
            {
                return;
            }

            for (var index = 0; index < parameters.Count; index++)
            {
                AddParameter(parameters[index]);
            }
        }

        public void AddParameter(AnimatorControllerParameter parameter)
        {
            if (null == parameter || string.IsNullOrEmpty(parameter.name) || null != GetParameter(parameter.name))
            {
                return;
            }

            m_Parameters.Add(parameter);
        }

        public void RemoveParameter(string name)
        {
            var parameter = GetParameter(name);
            if (null == parameter)
            {
                return;
            }

            m_Parameters.Remove(parameter);
        }

        public AnimatorControllerParameter GetParameter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (var index = 0; index < m_Parameters.Count; index++)
            {
                var parameter = m_Parameters[index];
                if (parameter.name == name)
                {
                    return parameter;
                }
            }

            return null;
        }

        public AnimatorControllerParameter GetParameter(int index)
        {
            if (index < 0 || index >= parametersCount)
            {
                return null;
            }

            return m_Parameters[index];
        }

        public AnimationMixerPlayable RebuildPlayable(PlayableAnimator animator, PlayableGraph graph)
        {
            playableGraph = graph;
            playableAnimator = animator;
            unityAnimator = animator?.animator;
            rootMixerPlayable = AnimationMixerPlayable.Create(graph, 1);

            RebuildPlayable();
            return rootMixerPlayable;
        }

        private void RebuildPlayable()
        {
            if (!rootMixerPlayable.IsValid())
            {
                return;
            }

            m_IsValid = true;
            m_LayerMixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, layersCount);
            rootMixerPlayable.DisconnectInput(0);
            rootMixerPlayable.ConnectInput(0, m_LayerMixerPlayable, 0, 1);

            for (var i = 0; i < layersCount; i++)
            {
                var layer = m_Layers[i];
                var layerIndex = i;
                layer.RebuildPlayable(this, layerIndex, (updatedType, stateName) =>
                {
                    stateNotify.Invoke(layerIndex, updatedType, stateName);
                    playableAnimator?.stateNotify?.Invoke(layerIndex, updatedType, stateName);
                });
            }
        }

        public void RecollectAnimationClips()
        {
            if (null == m_AnimationClips)
            {
                m_AnimationClips = new List<AnimationClip>();
            }
            else
            {
                m_AnimationClips.Clear();
            }

            for (var i = 0; i < layersCount; i++)
            {
                m_Layers[i].GetAnimationClips(m_AnimationClips);
            }
        }
    }

    public enum StateNotifyType
    {
        /// <summary>
        /// 状态准备进入
        /// </summary>
        PrepEnter = 0,

        /// <summary>
        /// 状态进入
        /// </summary>
        Enter,

        /// <summary>
        /// 状态准备退出
        /// </summary>
        PrepExit,

        /// <summary>
        /// 状态退出
        /// </summary>
        Exit,

        /// <summary>
        /// 动画播放完毕
        /// </summary>
        Complete
    }

    public class StateNotifyEvent : UnityEvent<int, StateNotifyType, string>
    {
    }

    public class TickEvent : UnityEvent
    {
    }
}
