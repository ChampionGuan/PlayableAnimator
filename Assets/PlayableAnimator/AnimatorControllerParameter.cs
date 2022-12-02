using System;
using UnityEngine;

namespace X3.PlayableAnimator
{
    public enum AnimatorControllerParameterType
    {
        Float = 1,
        Int = 3,
        Bool = 4,
        Trigger = 9
    }

    [Serializable]
    public class AnimatorControllerParameter
    {
        [SerializeField] private string m_Name;
        [SerializeField] private AnimatorControllerParameterType m_Type;
        [SerializeField] private int m_DefaultInt;
        [SerializeField] private float m_DefaultFloat;
        [SerializeField] private bool m_DefaultBool;

        public Action<AnimatorControllerParameter> m_OnValueChanged;
        public AnimatorController animCtrl { get; private set; }
        public string name => m_Name;
        public AnimatorControllerParameterType type => m_Type;

        public float defaultFloat
        {
            get => m_DefaultFloat;
            set
            {
                if (m_Type != AnimatorControllerParameterType.Float)
                {
                    return;
                }

                m_DefaultFloat = value;
                m_OnValueChanged?.Invoke(this);
            }
        }

        public int defaultInt
        {
            get => m_DefaultInt;
            set
            {
                if (m_Type != AnimatorControllerParameterType.Int)
                {
                    return;
                }

                m_DefaultInt = value;
                m_OnValueChanged?.Invoke(this);
            }
        }

        public bool defaultBool
        {
            get => m_DefaultBool;
            set
            {
                if (m_Type != AnimatorControllerParameterType.Bool && m_Type != AnimatorControllerParameterType.Trigger)
                {
                    return;
                }

                m_DefaultBool = value;
                m_OnValueChanged?.Invoke(this);
            }
        }

        public AnimatorControllerParameter(string name, AnimatorControllerParameterType type)
        {
            m_Name = name;
            m_Type = type;
        }

        public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, object value)
        {
            m_Name = name;
            m_Type = type;
            switch (type)
            {
                case AnimatorControllerParameterType.Trigger:
                    m_DefaultBool = false;
                    break;
                case AnimatorControllerParameterType.Bool:
                    m_DefaultBool = value is bool ? (bool) value : false;
                    break;
                case AnimatorControllerParameterType.Float:
                    m_DefaultFloat = value is float ? (float) value : 0;
                    break;
                case AnimatorControllerParameterType.Int:
                    m_DefaultInt = value is int ? (int) value : 0;
                    break;
            }
        }

        public void OnStart()
        {
        }

        public void OnUpdate(AnimatorController ctrl)
        {
            animCtrl = ctrl;
        }

        public void OnDestroy()
        {
        }

        public AnimatorControllerParameter DeepCopy()
        {
            var parameter = new AnimatorControllerParameter(m_Name, m_Type)
            {
                m_DefaultInt = m_DefaultInt,
                m_DefaultFloat = m_DefaultFloat,
                m_DefaultBool = m_DefaultBool
            };
            return parameter;
        }
    }
}
