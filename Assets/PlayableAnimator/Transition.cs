using System;
using System.Collections.Generic;
using UnityEngine;

namespace X3.PlayableAnimator
{
    public enum TransitionInterruptionSource
    {
        None,
        Source,
        Destination,
        SourceThenDestination,
        DestinationThenSource,
    }

    [Serializable]
    public class Transition
    {
        [SerializeField] private TransitionType m_Type = TransitionType.Standard;
        [SerializeField] private bool m_Solo;
        [SerializeField] private bool m_Mute;
        [SerializeField] private bool m_HasExitTime;
        [SerializeField] private bool m_HasFixedDuration;
        [SerializeField] private float m_Offset;
        [SerializeField] private float m_Duration;
        [SerializeField] private float m_ExitTime;
        [SerializeField] private string m_DestinationStateName;
        [SerializeField] private TransitionInterruptionSource m_InterruptionSource;
        [SerializeField] private bool m_OrderedInterruption;
        [SerializeField] private List<Condition> m_Conditions = new List<Condition>();

        public TransitionType type => m_Type;
        public bool solo => m_Solo;
        public bool mute => m_Mute;
        public bool hasExitTime => m_HasExitTime;
        public bool hasFixedDuration => m_HasFixedDuration;
        public float offset => m_Offset;
        public float duration => m_Duration;
        public float exitTime => m_ExitTime;
        public string destinationStateName => m_DestinationStateName;
        public TransitionInterruptionSource interruptionSource => m_InterruptionSource;
        public bool orderedInterruption => m_OrderedInterruption;
        public int conditionsCount => m_Conditions.Count;

        public Transition()
        {
        }

        public Transition(bool solo, bool mute, bool hasExitTime, bool hasFixedDuration, float offset, float duration, float exitTime, string destinationStateName, TransitionInterruptionSource interruptionSource, bool orderedInterruption, List<Condition> conditions)
        {
            m_Solo = solo;
            m_Mute = mute;
            m_HasExitTime = hasExitTime;
            m_HasFixedDuration = hasFixedDuration;
            m_Offset = offset;
            m_Duration = duration;
            m_ExitTime = exitTime;
            m_DestinationStateName = destinationStateName;
            m_InterruptionSource = interruptionSource;
            m_OrderedInterruption = orderedInterruption;
            m_Conditions = conditions ?? new List<Condition>();
            m_Type = TransitionType.Standard;
        }

        public void AddCondition(List<Condition> conditions)
        {
            if (null == conditions)
            {
                return;
            }

            for (var index = 0; index < conditions.Count; index++)
            {
                m_Conditions.Add(conditions[index]);
            }
        }

        public void AddCondition(Condition condition)
        {
            if (null == condition)
            {
                return;
            }

            m_Conditions.Add(condition);
        }

        public Condition GetCondition(int index)
        {
            if (index < 0 || index > conditionsCount)
            {
                return null;
            }

            return m_Conditions[index];
        }

        public void RemoveCondition(Condition condition)
        {
            if (null == condition)
            {
                return;
            }

            m_Conditions.Remove(condition);
        }

        public void RemoveCondition(int index)
        {
            m_Conditions.RemoveAt(index);
        }

        public void ClearCondition()
        {
            m_Conditions.Clear();
        }

        public Transition DeepCopy()
        {
            var transition = new Transition
            {
                m_Type = m_Type,
                m_Solo = m_Solo,
                m_Mute = m_Mute,
                m_HasExitTime = m_HasExitTime,
                m_HasFixedDuration = m_HasFixedDuration,
                m_Offset = m_Offset,
                m_Duration = m_Duration,
                m_ExitTime = m_ExitTime,
                m_DestinationStateName = m_DestinationStateName,
                m_InterruptionSource = m_InterruptionSource,
                m_OrderedInterruption = m_OrderedInterruption,
                m_Conditions = new List<Condition>()
            };
            for (var i = 0; i < conditionsCount; i++)
            {
                transition.m_Conditions.Add(m_Conditions[i].DeepCopy());
            }

            return transition;
        }

        public bool CanToDestinationState(AnimatorController ctrl, bool onlySolo, float time, float prevTime, float length, out float dValue)
        {
            dValue = 0;
            if (type != TransitionType.Standard)
            {
                return false;
            }

            if (onlySolo && !m_Solo)
            {
                return false;
            }

            if (m_Mute)
            {
                return false;
            }

            if (m_HasExitTime)
            {
                return CheckConditions(ctrl, time, prevTime) && AnimatorController.IsReachingThreshold(time, prevTime, length, m_ExitTime * length, ref dValue);
            }
            else
            {
                return CheckConditions(ctrl, time, prevTime);
            }
        }

        public bool IsForDestinationStateFade(string destStateName, out float fadeTime, out TransitionInterruptionSource interruptionSource)
        {
            interruptionSource = TransitionInterruptionSource.None;
            if (destinationStateName == destStateName && type == TransitionType.UseForFade)
            {
                interruptionSource = m_InterruptionSource;
                fadeTime = duration;
                return true;
            }

            fadeTime = 0;
            return false;
        }

        public void ResetConditions()
        {
            foreach (var condition in m_Conditions)
            {
                condition.Reset();
            }
        }

        private bool CheckConditions(AnimatorController ctrl, double time, double prevTime)
        {
            if (time == prevTime)
            {
                return false;
            }

            if (conditionsCount < 1)
            {
                return true;
            }

            for (var index = 0; index < m_Conditions.Count; index++)
            {
                var condition = m_Conditions[index];
                if (!condition.IsMeet(ctrl))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 连线类型
        /// </summary>
        public enum TransitionType
        {
            /// <summary>
            /// 用于内部状态过渡的标准连线
            /// </summary>
            Standard = 0,

            /// <summary>
            /// 用于两状态融合，取duration即可
            /// </summary>
            UseForFade,
        }
    }
}
