using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace X3.PlayableAnimator
{
    [Serializable]
    public class ClipMotion : Motion
    {
        [SerializeField] public AnimationClip m_Clip;

        protected float m_CacheLength = -1;

        public AnimatorController animCtrl { get; protected set; }
        public Playable clipPlayable { get; protected set; }
        public bool isValid { get; protected set; }
        public bool legacy => null != m_Clip && m_Clip.legacy;
        public override bool isLooping => null != m_Clip && m_Clip.isLooping;
        public override float length => m_CacheLength > 0 ? m_CacheLength : m_CacheLength = null == m_Clip ? 1 : m_Clip.length;

        public AnimationClip clip
        {
            get => m_Clip;
            set
            {
                if (m_Clip == value)
                {
                    return;
                }

                isValid = false;
                m_CacheLength = -1;
                m_Clip = value;
            }
        }

        public override double normalizedTime
        {
            get
            {
                var time = clipPlayable.GetTime();
                if (!isLooping) return time / length;
                return time % length / length;
            }
        }

        public ClipMotion(AnimationClip clip)
        {
            m_Clip = clip;
        }

        public override void SetTime(double time)
        {
            clipPlayable.SetTime(time);
            base.SetTime(time);
        }

        public override void Reset()
        {
            isValid = false;
            animCtrl = null;
        }

        public override Motion DeepCopy()
        {
            return new ClipMotion(m_Clip);
        }

        public override void RebuildPlayable(AnimatorController ctrl, AnimationMixerPlayable parent, int inputIndex)
        {
            isValid = true;
            animCtrl = ctrl;
            clipPlayable = AnimationClipPlayable.Create(parent.GetGraph(), m_Clip);
            
            parent.DisconnectInput(inputIndex);
            parent.ConnectInput(inputIndex, clipPlayable, 0, 1);
        }
    }
}
