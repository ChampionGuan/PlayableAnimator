using System;
using UnityEngine;

namespace X3.PlayableAnimator
{
    [Serializable]
    public class BlendTreeChild : ClipMotion
    {
        [SerializeField] private float m_Threshold;
        [SerializeField] private float m_TimeScale;
        [SerializeField] private float m_CycleOffset;
        [SerializeField] private string m_DirectBlendParameter;
        [SerializeField] private bool m_Mirror;

        public float threshold => m_Threshold;
        public float timeScale => m_TimeScale;
        public float cycleOffset => m_CycleOffset;
        public string directBlendParameter => m_DirectBlendParameter;
        public bool mirror => m_Mirror;

        public BlendTreeChild(AnimationClip clip) : base(clip)
        {
        }

        public BlendTreeChild(float threshold, float timeScale, float cycleOffset, string directBlendParameter, bool mirror, AnimationClip clip) : base(clip)
        {
            m_Threshold = threshold;
            m_TimeScale = timeScale;
            m_CycleOffset = cycleOffset;
            m_DirectBlendParameter = directBlendParameter;
            m_Mirror = mirror;
        }

        public override Motion DeepCopy()
        {
            var blendTreeChild = new BlendTreeChild(clip)
            {
                m_Threshold = m_Threshold,
                m_TimeScale = m_TimeScale,
                m_CycleOffset = m_CycleOffset,
                m_DirectBlendParameter = m_DirectBlendParameter,
                m_Mirror = m_Mirror
            };
            return blendTreeChild;
        }
    }
}
