using System;
using UnityEngine;

namespace X3.PlayableAnimator
{
    public enum ConditionType
    {
        If = 1,
        IfNot = 2,
        Greater = 3,
        Less = 4,
        Equals = 6,
        NotEqual = 7
    }

    [Serializable]
    public class Condition
    {
        [SerializeField] private ConditionType m_Type;
        [SerializeField] private string m_ParameterName;
        [SerializeField] private float m_Threshold;

        private AnimatorControllerParameter m_Parameter;

        public ConditionType type => m_Type;
        public string parameterName => m_ParameterName;
        public float threshold => m_Threshold;

        public Condition()
        {
        }

        public Condition(ConditionType type, string parameterName, float threshold)
        {
            m_Type = type;
            m_ParameterName = parameterName;
            m_Threshold = threshold;
        }

        public void Reset()
        {
            if (m_Parameter.type == AnimatorControllerParameterType.Trigger)
            {
                m_Parameter.defaultBool = false;
            }
        }

        public bool IsMeet(AnimatorController ctrl)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                return false;
            }

            if (null == m_Parameter || m_Parameter?.animCtrl != ctrl)
            {
                m_Parameter = ctrl.GetParameter(parameterName);
            }

            if (null == m_Parameter)
            {
                // Debug.Log($"[playable animator][frameCount:{Time.frameCount}][parameter not found, please check!!][parameterName:{parameterName}]");
                return false;
            }

            if (m_Parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return m_Parameter.defaultBool;
            }

            switch (type)
            {
                case ConditionType.If:
                    return m_Parameter.type == AnimatorControllerParameterType.Bool && m_Parameter.defaultBool;
                case ConditionType.IfNot:
                    return m_Parameter.type == AnimatorControllerParameterType.Bool && !m_Parameter.defaultBool;
                case ConditionType.Equals:
                    return m_Parameter.type == AnimatorControllerParameterType.Int && m_Parameter.defaultInt == threshold;
                case ConditionType.NotEqual:
                    return m_Parameter.type == AnimatorControllerParameterType.Int && m_Parameter.defaultInt != threshold;
                case ConditionType.Greater:
                    return (m_Parameter.type == AnimatorControllerParameterType.Float && m_Parameter.defaultFloat > threshold) || (m_Parameter.type == AnimatorControllerParameterType.Int && m_Parameter.defaultInt > threshold);
                case ConditionType.Less:
                    return (m_Parameter.type == AnimatorControllerParameterType.Float && m_Parameter.defaultFloat < threshold) || (m_Parameter.type == AnimatorControllerParameterType.Int && m_Parameter.defaultInt < threshold);
            }

            return false;
        }

        public Condition DeepCopy()
        {
            var condition = new Condition
            {
                m_Type = m_Type,
                m_ParameterName = m_ParameterName,
                m_Threshold = m_Threshold
            };
            return condition;
        }
    }
}
