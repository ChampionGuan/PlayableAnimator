using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;

namespace X3.PlayableAnimator
{
    public enum StateMotionType
    {
        Clip = 0,
        BlendTree,
        External
    }

    public struct AnimatorStateInfo
    {
        public string name { get; }
        public string tag { get; }
        public bool isLooping { get; }
        public float length { get; }
        public double normalizedTime { get; }
        public float speed { get; }

        public AnimatorStateInfo(string name, string tag, bool isLooping, float length, double normalizedTime, float speed)
        {
            this.name = name;
            this.tag = tag;
            this.isLooping = isLooping;
            this.length = length;
            this.normalizedTime = normalizedTime;
            this.speed = speed;
        }
    }

    [Serializable]
    public class StateMotion : StatePlayable
    {
        [HideInInspector] [SerializeField] private int m_HashID;
        [SerializeField] private float m_DefaultSpeed = 1;
        [SerializeField] private string m_SpeedParameterName;
        [SerializeField] private bool m_SpeedParameterActive;
        [SerializeField] private bool m_FootIK;
        [SerializeField] private bool m_WriteDefaultValues;
        [SerializeField] private StateMotionType m_MotionType;
        [SerializeField] private BlendTree m_BlendTree;
        [SerializeField] private ClipMotion m_ClipMotion;
        [SerializeField] private Motion m_ExternalMotion;

        public string speedParameterName => m_SpeedParameterName;
        public bool speedParameterActive => m_SpeedParameterActive;
        public bool footIK => m_FootIK;
        public bool writeDefaultValues => m_WriteDefaultValues;
        public bool isBlendTree => m_MotionType == StateMotionType.BlendTree;
        public int internalHashID => m_HashID;
        public AnimatorControllerLayer animLayer { get; private set; }
        public AnimatorController animCtrl => animLayer?.animCtrl;
        public virtual AnimationMixerPlayable mixerPlayable { get; protected set; }
        public virtual int internalIndex { get; protected set; }
        public override float internalWeight { get; protected set; }
        public override bool isValid { get; protected set; }
        public override bool isRunning => m_Status != InternalStatusType.Exit;
        public override bool isLooping => motion.isLooping;
        public override float speed => m_Speed;
        public override float length => motion.length > 0 ? motion.length : 1;
        public override double normalizedTime => motion.normalizedTime;

        private Action<string, Transition, float, float> m_ToDestinationStateAction;
        private Action<StateNotifyType, string> m_StateNotifyEvent;
        private List<StateGroup> m_StateGroups = new List<StateGroup>();
        private InternalStatusType m_Status = InternalStatusType.Exit;
        private StatePlayable m_PrevState;
        private StatePlayable m_NextState;

        private float m_EntryTime;
        private float m_EntryTimeOffset;
        private float m_RunningTime;
        private float m_RunningPrevTime;
        private float m_FadeOutTick;
        private float m_CacheLength;
        private float m_Speed;

        public override Motion motion
        {
            get
            {
                if (m_MotionType == StateMotionType.Clip)
                    return m_ClipMotion;
                if (m_MotionType == StateMotionType.BlendTree)
                    return m_BlendTree;
                if (m_MotionType == StateMotionType.External)
                    return m_ExternalMotion;
                return null;
            }
        }

        public StateMotion(Vector2 position, string name, string tag) : base(position, name, tag)
        {
            m_HashID = GetHashCode();
        }

        public StateMotion(Vector2 position, string name, string tag, float defaultSpeed, string speedParameterName, bool speedParameterActive, bool footIK, bool writeDefaultValues, Motion motion, List<Transition> transitions) : base(position, name, tag)
        {
            switch (motion)
            {
                case ClipMotion clipMotion:
                    m_ClipMotion = clipMotion;
                    m_MotionType = StateMotionType.Clip;
                    break;
                case BlendTree blendTree:
                    m_BlendTree = blendTree;
                    m_MotionType = StateMotionType.BlendTree;
                    break;
                default:
                    m_ExternalMotion = motion;
                    m_MotionType = StateMotionType.External;
                    break;
            }

            m_DefaultSpeed = defaultSpeed;
            m_SpeedParameterName = speedParameterName;
            m_SpeedParameterActive = speedParameterActive;
            m_FootIK = footIK;
            m_WriteDefaultValues = writeDefaultValues;
            m_Transitions = transitions ?? new List<Transition>();
            m_HashID = GetHashCode();
        }

        public override void OnEnter(float enterTime, StatePlayable prevState)
        {
            if (isRunning)
            {
                return;
            }

            internalWeight = 0;
            m_PrevState = prevState;
            m_EntryTimeOffset = 0;
            m_EntryTime = m_RunningTime = m_RunningPrevTime = enterTime;
            m_CacheLength = length;
            m_Status = InternalStatusType.PrepEnter;

            m_StateNotifyEvent?.Invoke(StateNotifyType.PrepEnter, name);
            animLayer.GetGroups(name, ref m_StateGroups);
            motion?.OnPrepEnter(prevState?.motion);
        }

        public override void OnUpdate(float deltaTime, float weight)
        {
            if (!isRunning)
            {
                return;
            }

            if (m_Status == InternalStatusType.PrepEnter)
            {
                OnEnterComplete();
            }

            deltaTime *= m_Speed;
            if (m_CacheLength != length)
            {
                m_EntryTimeOffset = (m_EntryTime + m_EntryTimeOffset) / m_CacheLength * length - m_EntryTime;
                m_CacheLength = length;
            }

            m_EntryTime += deltaTime;
            m_RunningPrevTime = m_RunningTime;
            m_RunningTime = m_EntryTime + m_EntryTimeOffset;
            if (deltaTime != 0)
            {
                motion?.SetTime(m_RunningTime);
            }

            if (m_Status == InternalStatusType.PrepExit && (m_FadeOutTick -= deltaTime) <= 0)
            {
                OnExitComplete();
            }
            else
            {
                SetWeight(weight);
            }

            float temp = 0;
            if (AnimatorController.IsReachingThreshold(m_RunningTime, m_RunningPrevTime, length, length, ref temp))
            {
                m_StateNotifyEvent?.Invoke(StateNotifyType.Complete, name);
            }

            if (isRunning)
            {
                CheckToDestinationState();
            }
        }

        public override void OnExit(float fadeOutDuration, StatePlayable nextState)
        {
            if (!isRunning)
            {
                return;
            }

            m_NextState = nextState;
            m_FadeOutTick = fadeOutDuration;
            m_Status = fadeOutDuration > 0 ? InternalStatusType.PrepExit : InternalStatusType.Exit;

            m_StateNotifyEvent?.Invoke(StateNotifyType.PrepExit, name);
            motion?.OnPrepExit(nextState?.motion);

            if (m_Status == InternalStatusType.Exit)
            {
                OnExitComplete();
            }
        }

        private void OnEnterComplete()
        {
            m_Status = InternalStatusType.Enter;
            m_StateNotifyEvent?.Invoke(StateNotifyType.Enter, name);
            if (null != motion)
            {
                motion.OnEnter(m_PrevState?.motion);
                motion.SetTime(m_EntryTime);
                motion.SetTime(m_EntryTime);
            }
        }

        private void OnExitComplete()
        {
            SetWeight(0);
            m_Status = InternalStatusType.Exit;
            m_StateNotifyEvent?.Invoke(StateNotifyType.Exit, name);
            motion?.OnExit(m_NextState?.motion);
        }

        public override void OnDestroy()
        {
            motion?.OnDestroy();
        }

        public override void SetWeight(float weight, bool syncToPlayable = true)
        {
            if (!isValid) return;
            internalWeight = weight;

            if (!syncToPlayable || animLayer.mixerPlayable.GetInputWeight(internalIndex) == internalWeight) return;
            animLayer.mixerPlayable.SetInputWeight(internalIndex, internalWeight);
            motion?.SetWeight(internalWeight);
        }

        public override void SetSpeed(float speed)
        {
            m_Speed = m_DefaultSpeed = speed;
            m_SpeedParameterActive = false;
        }

        public override void Reset()
        {
            m_Status = InternalStatusType.Exit;
            m_ToDestinationStateAction = null;
            m_StateNotifyEvent = null;
            animLayer?.mixerPlayable.DisconnectInput(internalIndex);
            animLayer = null;
            isValid = false;
        }

        public void CheckToDestinationState()
        {
            if (CheckToDestinationState(this))
            {
                return;
            }

            for (var index = 0; index < m_StateGroups.Count; index++)
            {
                if (CheckToDestinationState(m_StateGroups[index]))
                {
                    return;
                }
            }
        }

        public StatePlayable DeepCopy()
        {
            var state = new StateMotion(m_Position, m_Name, m_Tag)
            {
                m_HashID = m_HashID,
                m_DefaultSpeed = m_DefaultSpeed,
                m_SpeedParameterName = m_SpeedParameterName,
                m_SpeedParameterActive = m_SpeedParameterActive,
                m_FootIK = m_FootIK,
                m_WriteDefaultValues = m_WriteDefaultValues,
                m_MotionType = m_MotionType,
                m_ClipMotion = m_ClipMotion?.DeepCopy() as ClipMotion,
                m_BlendTree = m_BlendTree?.DeepCopy() as BlendTree,
                m_ExternalMotion = m_ExternalMotion?.DeepCopy(),
                m_Transitions = new List<Transition>()
            };

            for (var i = 0; i < transitionsCount; i++)
            {
                state.m_Transitions.Add(m_Transitions[i].DeepCopy());
            }

            return state;
        }

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (null == clips)
            {
                return;
            }

            if (m_MotionType == StateMotionType.Clip)
            {
                var clip = m_ClipMotion?.clip;
                if (null != clip && !clips.Contains(clip))
                {
                    clips.Add(clip);
                }
            }
            else if (m_MotionType == StateMotionType.BlendTree)
            {
                m_BlendTree.GetAnimationClips(clips);
            }
        }

        public virtual void RebuildPlayable(AnimatorControllerLayer layer, int inputIndex, Action<string, Transition, float, float> toDestinationStateInvoke, Action<StateNotifyType, string> stateNotify)
        {
            isValid = true;
            animLayer = layer;
            internalIndex = inputIndex;
            mixerPlayable = AnimationMixerPlayable.Create(animCtrl.playableGraph, 1);
            animLayer.mixerPlayable.DisconnectInput(internalIndex);
            animLayer.mixerPlayable.ConnectInput(internalIndex, mixerPlayable, 0, internalWeight);

            m_Speed = m_DefaultSpeed;
            m_StateNotifyEvent = stateNotify;
            m_ToDestinationStateAction = toDestinationStateInvoke;
            motion?.RebuildPlayable(animCtrl, mixerPlayable, 0);

            if (!m_SpeedParameterActive) return;
            var parameter = animCtrl?.GetParameter(m_SpeedParameterName);
            if (null != parameter)
            {
                parameter.m_OnValueChanged -= OnParameterValueChanged;
                parameter.m_OnValueChanged += OnParameterValueChanged;
                m_Speed *= parameter.defaultFloat;
            }
            else
            {
                // Debug.Log($"[playable animator][frameCount:{Time.frameCount}][parameter not found, please check!!][parameterName:{m_SpeedParameterName}]");
            }
        }

        private bool CheckToDestinationState(State state)
        {
            var onlySolo = false;
            for (var index = 0; index < state.transitions.Count; index++)
            {
                if (state.transitions[index].solo)
                {
                    onlySolo = true;
                    break;
                }
            }

            for (var index = 0; index < state.transitions.Count; index++)
            {
                var transition = state.transitions[index];
                if (!transition.CanToDestinationState(animCtrl, onlySolo, m_RunningTime, m_RunningPrevTime, length, out var dValue))
                {
                    continue;
                }

                if (!animLayer.HasMotionState(transition.destinationStateName))
                {
                    continue;
                }

                m_ToDestinationStateAction?.Invoke(name, transition, dValue / speed, transition.hasFixedDuration ? transition.duration / length : transition.duration);
                return true;
            }

            return false;
        }

        private void OnParameterValueChanged(AnimatorControllerParameter parameter)
        {
            if (null == parameter || m_SpeedParameterName != parameter.name || !m_SpeedParameterActive)
            {
                return;
            }

            m_Speed = m_DefaultSpeed * parameter.defaultFloat;
        }
    }
}
