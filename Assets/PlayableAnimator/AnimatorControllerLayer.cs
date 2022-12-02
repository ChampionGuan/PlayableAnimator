using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace X3.PlayableAnimator
{
    public enum AnimatorControllerLayerBlendingType
    {
        Override = 0,
        Additive = 1
    }

    [Serializable]
    public class AnimatorControllerLayer
    {
        public static int Default_Capacity = 0;

        [SerializeField] private string m_Name;
        [SerializeField] private float m_DefaultSpeed = 1;
        [SerializeField] private float m_DefaultWeight;
        [SerializeField] private bool m_IKPass;
        [SerializeField] private bool m_SyncedLayerAffectsTiming;
        [SerializeField] private int m_SyncedLayerIndex;
        [SerializeField] private string m_DefaultStateName;
        [SerializeField] private AvatarMask m_AvatarMask;
        [SerializeField] private AnimatorControllerLayerBlendingType m_BlendingType;
        [SerializeField] private List<StateMotion> m_States = new List<StateMotion>();
        [SerializeField] private List<StateGroup> m_Groups = new List<StateGroup>();

        public string name => m_Name;
        public bool ikPass => m_IKPass;
        public bool syncedLayerAffectsTiming => m_SyncedLayerAffectsTiming;
        public int syncedLayerIndex => m_SyncedLayerIndex;
        public string defaultStateName => m_DefaultStateName;
        public float defaultWeight => m_DefaultWeight;
        public AvatarMask avatarMask => m_AvatarMask;
        public AnimatorControllerLayerBlendingType blendingType => m_BlendingType;
        public int statesCount => m_States.Count;
        public int groupsCount => m_Groups.Count;

        public bool isValid { get; private set; }
        public StatePlayable oldestState { get; private set; }
        public StatePlayable prevState { get; private set; }
        public StatePlayable currState { get; private set; }
        public AnimatorController animCtrl { get; private set; }
        public AnimationMixerPlayable mixerPlayable { get; private set; }
        public int internalIndex { get; private set; }

        private List<StateMotion> m_DynamicStates;
        private List<StateGroup> m_CurrStateGroups;
        private List<StateGroup> m_DestStateGroups;
        private Action<StateNotifyType, string> m_StateNotifyEvent;
        private Action<string, Transition, float, float> m_ToDestinationStateAction;
        private TransitionInterruptionSource m_InterruptionSource;
        private StatePose m_StatePose;
        private ToState m_StateNext;
        private float m_BlendTime;
        private float m_BlendTick;
        private int m_InputCount;

        public AnimatorControllerLayer(string name, float defaultWeight, AnimatorControllerLayerBlendingType blendingType)
        {
            m_Name = name;
            m_DefaultWeight = defaultWeight;
            m_BlendingType = blendingType;
        }

        public AnimatorControllerLayer(string name, float defaultSpeed, float defaultWeight, bool iKPass, bool syncedLayerAffectsTiming, int syncedLayerIndex, string defaultStateName, AvatarMask avatarMask, AnimatorControllerLayerBlendingType blendingType, List<StateMotion> states, List<StateGroup> groups)
        {
            m_Name = name;
            m_DefaultSpeed = defaultSpeed;
            m_DefaultWeight = defaultWeight;
            m_IKPass = iKPass;
            m_SyncedLayerAffectsTiming = syncedLayerAffectsTiming;
            m_SyncedLayerIndex = syncedLayerIndex;
            m_DefaultStateName = defaultStateName;
            m_AvatarMask = avatarMask;
            m_BlendingType = blendingType;
            m_States = states ?? new List<StateMotion>();
            m_Groups = groups ?? new List<StateGroup>();
        }

        public void OnStart()
        {
            if (null != currState)
            {
                return;
            }

            SwitchState(true, TransitionInterruptionSource.None, null, m_DefaultStateName, 0, 0, 0, null);
        }

        public void OnUpdate(float deltaTime)
        {
            deltaTime *= m_DefaultSpeed;
            if (m_StateNext.Switch())
            {
                BlendState(deltaTime, 0, deltaTime);
            }
            else
            {
                BlendState(deltaTime, deltaTime, deltaTime);
            }
        }

        public void OnDestroy()
        {
            for (var i = 0; i < m_States.Count; i++)
            {
                m_States[i].OnDestroy();
            }

            for (var i = 0; null != m_DynamicStates && i < m_DynamicStates.Count; i++)
            {
                m_DynamicStates[i].OnDestroy();
            }
        }

        public void SetSpeed(float speed)
        {
            m_DefaultSpeed = speed;
        }

        public void Play(string stateName, float normalizedTime)
        {
            if (!CanToState(stateName, normalizedTime))
            {
                return;
            }

            m_StateNext.SetValue(true, TransitionInterruptionSource.None, null, stateName, 0, normalizedTime, 0);
        }

        public void PlayInFixedTime(string stateName, float fixedTime)
        {
            if (!CanToState(stateName, fixedTime))
            {
                return;
            }

            m_StateNext.SetValue(false, TransitionInterruptionSource.None, null, stateName, 0, fixedTime, 0);
        }

        public void CrossFade(string stateName, float normalizedOffsetTime, float dValue)
        {
            if (!CanToState(stateName, normalizedOffsetTime))
            {
                return;
            }

            m_StateNext.SetValue(true, TransitionInterruptionSource.None, null, stateName, dValue, normalizedOffsetTime, GetFadeInNormalizedTime(stateName));
        }

        public void CrossFade(string stateName, float normalizedOffsetTime, float normalizedTransitionTime, float dValue)
        {
            if (!CanToState(stateName, normalizedOffsetTime))
            {
                return;
            }

            m_StateNext.SetValue(true, TransitionInterruptionSource.None, null, stateName, dValue, normalizedOffsetTime, normalizedTransitionTime);
        }

        public void CrossFadeInFixedTime(string stateName, float fixedOffsetTime, float dValue)
        {
            if (!CanToState(stateName, fixedOffsetTime))
            {
                return;
            }

            var transitionTime = GetFadeInFixedTime(stateName, out var interruptionSource);
            m_StateNext.SetValue(false, interruptionSource, currState?.name, stateName, dValue, fixedOffsetTime, transitionTime);
        }

        public void CrossFadeInFixedTime(string stateName, float fixedOffsetTime, float fixedTransitionTime, float dValue)
        {
            if (!CanToState(stateName, fixedOffsetTime))
            {
                return;
            }

            m_StateNext.SetValue(false, TransitionInterruptionSource.None, null, stateName, dValue, fixedOffsetTime, fixedTransitionTime);
        }

        private void InternalCrossFade(string fromStateName, Transition transition, float dValue, float normalizedTransitionTime)
        {
            if (m_BlendTick > 0)
            {
                if (m_InterruptionSource == TransitionInterruptionSource.None)
                {
                    // Debug.Log($"[playable animator][frameCount:{Time.frameCount}][正在过渡状态，请稍候...]");
                    return;
                }

                if (m_InterruptionSource == TransitionInterruptionSource.Source && prevState.name != fromStateName)
                {
                    return;
                }

                if (m_InterruptionSource == TransitionInterruptionSource.Destination && currState.name != fromStateName)
                {
                    return;
                }
            }

            if (null != currState && transition.destinationStateName == currState.name)
            {
                return;
            }

            m_StateNext.SetValue(true, transition, fromStateName, dValue, normalizedTransitionTime);
            m_StateNext.Switch();
            // OnUpdate(0);
        }

        public float GetFadeInNormalizedTime(string destStateName)
        {
            var fadeTime = GetFadeInFixedTime(destStateName, out var interruptionSource);
            if (fadeTime <= 0)
            {
                return fadeTime;
            }

            var state = GetState(destStateName, m_States);
            if (null == state || state.length <= 0)
            {
                return 0;
            }

            return fadeTime / state.length;
        }

        public float GetFadeInFixedTime(string destStateName, out TransitionInterruptionSource interruptionSource)
        {
            var fadeTime = 0f;
            interruptionSource = TransitionInterruptionSource.None;
            if (null == currState)
            {
                return fadeTime;
            }

            var state = GetState(destStateName, m_States);
            if (null == state)
            {
                return fadeTime;
            }

            if (currState.HasFadeTransition(destStateName, out fadeTime, out interruptionSource))
            {
                return fadeTime;
            }

            GetGroups(currState.name, ref m_CurrStateGroups);
            GetGroups(destStateName, ref m_DestStateGroups);
            for (var i = 0; i < m_DestStateGroups.Count; i++)
            {
                var destGroup = m_DestStateGroups[i];
                if (currState.HasFadeTransition(destGroup.name, out fadeTime, out interruptionSource))
                {
                    return fadeTime;
                }
            }

            for (var index = 0; index < m_CurrStateGroups.Count; index++)
            {
                var currGroup = m_CurrStateGroups[index];
                if (currGroup.HasFadeTransition(destStateName, out fadeTime, out interruptionSource))
                {
                    return fadeTime;
                }

                for (var i = 0; i < m_DestStateGroups.Count; i++)
                {
                    var destGroup = m_DestStateGroups[i];
                    if (currGroup.HasFadeTransition(destGroup.name, out fadeTime, out interruptionSource))
                    {
                        return fadeTime;
                    }
                }
            }

            return fadeTime;
        }

        public void GetGroups(string stateName, ref List<StateGroup> groups)
        {
            if (null == groups)
            {
                groups = new List<StateGroup>();
            }
            else
            {
                groups.Clear();
            }

            if (string.IsNullOrEmpty(stateName))
            {
                return;
            }

            for (var index = 0; index < m_Groups.Count; index++)
            {
                var group = m_Groups[index];
                if (group.HasChild(stateName))
                {
                    groups.Add(group);
                }
            }
        }

        public T GetState<T>(string name) where T : State
        {
            if (typeof(T) == typeof(StateMotion))
            {
                return GetState(name, m_States) as T;
            }

            if (typeof(T) == typeof(StateGroup))
            {
                return GetState(name, m_Groups) as T;
            }

            return null;
        }

        public T GetState<T>(int index) where T : State
        {
            if (typeof(T) == typeof(StateMotion))
            {
                return GetState(index, m_States) as T;
            }

            if (typeof(T) == typeof(StateGroup))
            {
                return GetState(index, m_Groups) as T;
            }

            return null;
        }

        public bool AddState<T>(T state) where T : State
        {
            if (state is StateMotion motionState)
            {
                return AddState(motionState, m_States);
            }

            if (state is StateGroup groupState)
            {
                AddState(groupState, m_Groups);
            }

            return false;
        }

        public void AddState<T>(List<T> states) where T : State
        {
            if (null == states)
            {
                return;
            }

            for (var index = 0; index < states.Count; index++)
            {
                AddState(states[index]);
            }
        }

        public bool HasMotionState(string stateName)
        {
            return null != GetState(stateName, m_States);
        }

        public bool HasGroupState(string stateName)
        {
            return null != GetState(stateName, m_Groups);
        }

        public bool HasState<T>(string stateName) where T : State
        {
            if (typeof(T) == typeof(StateMotion))
            {
                return null != GetState(stateName, m_States);
            }

            if (typeof(T) == typeof(StateGroup))
            {
                return null != GetState(stateName, m_Groups);
            }

            return false;
        }

        public bool HasState(string stateName)
        {
            if (null != GetState(stateName, m_States))
            {
                return true;
            }

            return null != GetState(stateName, m_Groups);
        }

        public bool RemoveState(string stateName)
        {
            if (RemoveState(stateName, m_States))
            {
                return true;
            }

            return null != RemoveState(stateName, m_Groups);
        }

        public bool AddState(string stateName, Motion motion)
        {
            return AddState(new StateMotion(Vector3.zero, stateName, null, 1, null, false, false, false, motion, null), m_States);
        }

        public void SetStateSpeed(string stateName, float speed)
        {
            GetState(stateName, m_States)?.SetSpeed(speed);
        }

        public AnimatorStateInfo GetPreviousStateInfo()
        {
            return null != prevState ? new AnimatorStateInfo(prevState.name, prevState.tag, prevState.isLooping, prevState.length, prevState.normalizedTime, prevState.speed) : new AnimatorStateInfo();
        }

        public AnimatorStateInfo GetCurrentStateInfo()
        {
            return null != currState ? new AnimatorStateInfo(currState.name, currState.tag, currState.isLooping, currState.length, currState.normalizedTime, currState.speed) : new AnimatorStateInfo();
        }

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (null == clips)
            {
                return;
            }

            for (var index = 0; index < m_States.Count; index++)
            {
                m_States[index].GetAnimationClips(clips);
            }
        }

        public void RebuildPlayable(AnimatorController ctrl, int inputIndex, Action<StateNotifyType, string> stateNotify)
        {
            isValid = true;
            animCtrl = ctrl;
            internalIndex = inputIndex;

            m_InputCount = 0;
            m_StateNotifyEvent = stateNotify;
            m_StatePose = m_StatePose ?? new StatePose(this);
            m_StateNext = m_StateNext ?? new ToState(SwitchState);
            m_DynamicStates = m_DynamicStates ?? new List<StateMotion>(Default_Capacity);
            if (null == m_ToDestinationStateAction) m_ToDestinationStateAction = InternalCrossFade;

            mixerPlayable = AnimationMixerPlayable.Create(animCtrl.playableGraph, m_States.Count + m_DynamicStates.Count + Default_Capacity);
            var layerMixerPlayable = animCtrl.layerMixerPlayable;
            layerMixerPlayable.DisconnectInput(internalIndex);
            layerMixerPlayable.ConnectInput(internalIndex, mixerPlayable, 0, defaultWeight);
            layerMixerPlayable.SetLayerAdditive((uint) internalIndex, blendingType == AnimatorControllerLayerBlendingType.Additive);
            if (null != avatarMask) layerMixerPlayable.SetLayerMaskFromAvatarMask((uint) internalIndex, avatarMask);

            foreach (var state in m_States)
            {
                BuildStatePlayable(state);
            }

            foreach (var state in m_DynamicStates)
            {
                BuildStatePlayable(state);
            }
        }

        public void Reset()
        {
            isValid = false;
            animCtrl = null;
            m_StateNotifyEvent = null;
        }

        public AnimatorControllerLayer DeepCopy()
        {
            var layer = new AnimatorControllerLayer(m_Name, m_DefaultWeight, m_BlendingType)
            {
                m_DefaultSpeed = 1,
                m_IKPass = m_IKPass,
                m_SyncedLayerAffectsTiming = m_SyncedLayerAffectsTiming,
                m_SyncedLayerIndex = m_SyncedLayerIndex,
                m_DefaultStateName = m_DefaultStateName,
                m_AvatarMask = m_AvatarMask,
                m_BlendingType = m_BlendingType,
                m_States = new List<StateMotion>(),
                m_Groups = new List<StateGroup>()
            };

            for (var i = 0; i < statesCount; i++)
            {
                layer.m_States.Add(m_States[i].DeepCopy() as StateMotion);
            }

            for (var i = 0; i < groupsCount; i++)
            {
                layer.m_Groups.Add(m_Groups[i].DeepCopy());
            }

            return layer;
        }

        private bool CanToState(string stateName, float offsetTime)
        {
            if (offsetTime == float.NegativeInfinity && null != currState && currState.name == stateName)
            {
                // Debug.Log($"[playable animator][frameCount:{Time.frameCount}][当前状态正在播放][{currState.name}]");
                return false;
            }

            return true;
        }

        private T GetState<T>(string name, List<T> states) where T : State
        {
            if (string.IsNullOrEmpty(name) || null == states)
            {
                return default;
            }

            for (var index = 0; index < states.Count; index++)
            {
                var state = states[index];
                if (state.name == name)
                {
                    return state;
                }
            }

            return default;
        }

        private T GetState<T>(int index, List<T> states) where T : State
        {
            if (index < 0 || null == states || index >= states.Count)
            {
                return null;
            }

            return states[index];
        }

        private bool AddState<T>(T state, List<T> states) where T : State
        {
            if (null == state || null == states || null != GetState<T>(state.name))
            {
                return false;
            }

            state.Reset();
            states.Add(state);
            return true;
        }

        private bool RemoveState<T>(string name, List<T> states) where T : State
        {
            var state = GetState(name, states);
            if (null == state)
            {
                return false;
            }

            states.Remove(state);
            return true;
        }

        private void SwitchState(bool isNormalizedTime, TransitionInterruptionSource interruptionSource, string fromStateName, string destStateName, float dValue, float offsetTime, float transitionTime, Transition transition)
        {
            if (m_BlendTick > 0 && !string.IsNullOrEmpty(fromStateName))
            {
                if (fromStateName == currState.name)
                {
                    m_StatePose.Enter(currState as StateMotion, prevState, currState);
                }
                else if (fromStateName == prevState.name)
                {
                    m_StatePose.Enter(prevState as StateMotion, prevState, currState);
                }
                else
                {
                    // Debug.LogError($"[playable animator][frameCount:{Time.frameCount}][切换状态异常！][状态非法：{fromStateName},请检查!!]");
                }

                prevState = null;
                currState = m_StatePose;
            }

            if (isNormalizedTime)
            {
                SwitchStateInNormalizedTime(GetState(destStateName, m_States), interruptionSource, dValue, offsetTime, transitionTime, transition);
            }
            else
            {
                SwitchStateInFixedTime(GetState(destStateName, m_States), interruptionSource, dValue, offsetTime, transitionTime, transition);
            }
        }

        private void SwitchStateInNormalizedTime(StateMotion state, TransitionInterruptionSource interruptionSource, float dValue, float normalizedOffsetTime, float normalizedTransitionTime, Transition transition)
        {
            if (null == state)
            {
                return;
            }

            SwitchStateInFixedTime(state, interruptionSource, dValue, normalizedOffsetTime * state.length, normalizedTransitionTime * currState?.length ?? 0f, transition);
        }

        private void SwitchStateInFixedTime(StateMotion state, TransitionInterruptionSource interruptionSource, float dValue, float fixedOffsetTime, float fixedTransitionTime, Transition transition)
        {
            if (null == state)
            {
                return;
            }

            if (float.IsNaN(fixedOffsetTime) || fixedOffsetTime == float.NegativeInfinity)
            {
                fixedOffsetTime = 0;
            }

            if (fixedTransitionTime > 0 && state.isRunning)
            {
                foreach (var dynamicState in m_DynamicStates)
                {
                    if (dynamicState.isRunning || dynamicState.internalHashID != state.internalHashID) continue;
                    state = dynamicState;
                    break;
                }

                if (state.isRunning)
                {
                    state = state.DeepCopy() as StateMotion;
                    m_DynamicStates.Add(state);
                }
            }

            if (!state.isValid)
            {
                BuildStatePlayable(state);
            }

            // Debug.Log($"[playable animator][frameCount:{Time.frameCount}][播放状态][{state.name}][dValue:{dValue}]");
            m_BlendTime = m_BlendTick = fixedTransitionTime;
            m_InterruptionSource = interruptionSource;

            oldestState = prevState;
            prevState?.OnExit(0, state);
            prevState = currState;
            prevState?.OnExit(fixedTransitionTime - dValue, state);
            currState = state;
            currState?.OnEnter(fixedOffsetTime, prevState);
            prevState = prevState == currState ? oldestState : prevState;

            transition?.ResetConditions();
            BlendState(dValue, dValue, 0);
        }

        private void BlendState(float deltaTime, float currDelta, float prevDelta)
        {
            var prevStateWeight = 0f;
            var currStateWeight = 1f;
            if (m_BlendTick > 0)
            {
                m_BlendTick -= deltaTime;
                prevStateWeight = Mathf.Clamp01(m_BlendTick / m_BlendTime);
                currStateWeight = 1 - prevStateWeight;
            }
            else if (m_BlendTick == 0)
            {
                m_BlendTick -= deltaTime;
                prevStateWeight = 0;
                currStateWeight = 1 - prevStateWeight;
            }

            prevState?.SetWeight(prevStateWeight, false);
            currState?.SetWeight(currStateWeight, false);

            if (m_InterruptionSource == TransitionInterruptionSource.None || m_InterruptionSource == TransitionInterruptionSource.DestinationThenSource)
            {
                var state = currState;
                prevState?.OnUpdate(prevDelta, prevStateWeight);
                if (state != currState) return;
                currState?.OnUpdate(currDelta, currStateWeight);
            }
            else
            {
                var state = prevState;
                currState?.OnUpdate(currDelta, currStateWeight);
                if (state != prevState) return;
                prevState?.OnUpdate(prevDelta, prevStateWeight);
            }
        }

        private void BuildStatePlayable(StateMotion state)
        {
            if (null == state)
            {
                return;
            }

            if (m_InputCount > mixerPlayable.GetInputCount() - 1)
            {
                RebuildPlayable(animCtrl, internalIndex, m_StateNotifyEvent);
            }
            else
            {
                state.RebuildPlayable(this, m_InputCount++, m_ToDestinationStateAction, m_StateNotifyEvent);
            }
        }

        private class ToState
        {
            private string m_FromName;
            private string m_DestName;
            private float m_DValue;
            private float m_OffsetTime;
            private float m_TransitionTime;
            private bool m_IsNormalizedTime;
            private Transition m_Transition;
            private TransitionInterruptionSource m_InterruptionSource;
            private Action<bool, TransitionInterruptionSource, string, string, float, float, float, Transition> m_ToEvent;

            public ToState(Action<bool, TransitionInterruptionSource, string, string, float, float, float, Transition> action)
            {
                m_ToEvent = action;
            }

            public void SetValue(bool isNormalizedTime, TransitionInterruptionSource interruptionSource, string fromName, string destName, float dValue, float offsetTime, float transitionTime)
            {
                m_FromName = fromName;
                m_DestName = destName;
                m_DValue = dValue;
                m_OffsetTime = offsetTime;
                m_TransitionTime = transitionTime;
                m_IsNormalizedTime = isNormalizedTime;
                m_InterruptionSource = interruptionSource;
                m_Transition = null;
            }

            public void SetValue(bool isNormalizedTime, Transition transition, string fromState, float dValue, float transitionTime)
            {
                m_FromName = fromState;
                m_DestName = transition.destinationStateName;
                m_DValue = dValue;
                m_OffsetTime = transition.offset;
                m_TransitionTime = transitionTime;
                m_IsNormalizedTime = isNormalizedTime;
                m_InterruptionSource = transition.interruptionSource;
                m_Transition = transition;
            }

            public bool Switch()
            {
                if (string.IsNullOrEmpty(m_DestName) || null == m_ToEvent)
                {
                    return false;
                }

                var isNormalizedTime = m_IsNormalizedTime;
                var fromName = m_FromName;
                var destName = m_DestName;
                var dValue = m_DValue;
                var offsetTime = m_OffsetTime;
                var transitionTime = m_TransitionTime;
                var interruptionSource = m_InterruptionSource;
                var transition = m_Transition;
                m_FromName = null;
                m_DestName = null;
                m_DValue = 0;
                m_OffsetTime = 0;
                m_TransitionTime = 0;
                m_InterruptionSource = TransitionInterruptionSource.None;
                m_Transition = null;
                m_ToEvent?.Invoke(isNormalizedTime, interruptionSource, fromName, destName, dValue, offsetTime, transitionTime, transition);
                return true;
            }
        }
    }
}
