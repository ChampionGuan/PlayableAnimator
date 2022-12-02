using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace X3.PlayableAnimator
{
    public enum BlendTreeType
    {
        Simple1D,
        SimpleDirectional2D,
        FreeformDirectional2D,
        FreeformCartesian2D,
        Direct,
    }

    [Serializable]
    public class BlendTree : Motion
    {
        [SerializeField] private float m_MinThreshold;
        [SerializeField] private float m_MaxThreshold;
        [SerializeField] private string m_BlendParameterName;
        [SerializeField] private BlendTreeType m_BlendType;
        [SerializeField] private bool m_UseAutomaticThresholds;
        [SerializeField] private List<BlendTreeChild> m_ChildMotions = new List<BlendTreeChild>();

        private bool m_IsValid;
        private float m_Length;
        private int m_PlayableInputIndex;
        private AnimationMixerPlayable m_PlayableParent;

        public AnimatorController animCtrl { get; private set; }
        public AnimationMixerPlayable mixerPlayable { get; private set; }
        public override float length => m_Length;
        public override bool isLooping => false;
        public override double normalizedTime => mixerPlayable.GetTime() / length;
        public float minThreshold => m_MinThreshold;
        public float maxThreshold => m_MaxThreshold;
        public string blendParameterName => m_BlendParameterName;
        public BlendTreeType blendType => m_BlendType;
        public bool useAutomaticThresholds => m_UseAutomaticThresholds;
        public int childrenCount => m_ChildMotions.Count;

        public List<AnimationClip> animationClips
        {
            get
            {
                var clips = new List<AnimationClip>();
                for (var index = 0; index < m_ChildMotions.Count; index++)
                {
                    var motion = m_ChildMotions[index];
                    if (null != motion.clip && !clips.Contains(motion.clip))
                    {
                        clips.Add(motion.clip);
                    }
                }

                return clips;
            }
        }

        public BlendTree()
        {
        }

        public BlendTree(float minThreshold, float maxThreshold, string blendParameterName, BlendTreeType blendType, bool useAutomaticThresholds, List<BlendTreeChild> childMotions)
        {
            m_MinThreshold = minThreshold;
            m_MaxThreshold = maxThreshold;
            m_BlendParameterName = blendParameterName;
            m_BlendType = blendType;
            m_UseAutomaticThresholds = useAutomaticThresholds;
            m_ChildMotions = childMotions;
        }

        public void AddChild(BlendTreeChild[] motions)
        {
            if (null == motions)
            {
                return;
            }

            for (var index = 0; index < motions.Length; index++)
            {
                AddChild(motions[index]);
            }
        }

        public void AddChild(BlendTreeChild motion)
        {
            if (null == motion)
            {
                return;
            }

            motion.Reset();
            m_IsValid = false;
            m_ChildMotions.Add(motion);
        }

        public void RemoveChild(BlendTreeChild motion)
        {
            if (null == motion)
            {
                return;
            }

            m_IsValid = false;
            m_ChildMotions.Remove(motion);
        }

        public BlendTreeChild GetChild(int index)
        {
            if (index < 0 || index >= childrenCount)
            {
                return null;
            }

            return m_ChildMotions[index];
        }

        public override void SetTime(double time)
        {
            mixerPlayable.SetTime(time);
            for (var i = 0; i < childrenCount; i++)
            {
                m_ChildMotions[i].SetTime(normalizedTime * m_ChildMotions[i].length);
            }

            base.SetTime(time);
        }

        public override void Reset()
        {
            m_IsValid = false;
            animCtrl = null;
            for (var index = 0; index < m_ChildMotions.Count; index++)
            {
                m_ChildMotions[index].Reset();
            }
        }

        public override Motion DeepCopy()
        {
            var blendTree = new BlendTree
            {
                m_MinThreshold = m_MinThreshold,
                m_MaxThreshold = m_MaxThreshold,
                m_BlendParameterName = m_BlendParameterName,
                m_BlendType = m_BlendType,
                m_UseAutomaticThresholds = m_UseAutomaticThresholds,
                m_ChildMotions = new List<BlendTreeChild>()
            };
            for (var i = 0; i < childrenCount; i++)
            {
                blendTree.m_ChildMotions.Add(m_ChildMotions[i].DeepCopy() as BlendTreeChild);
            }

            return blendTree;
        }

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (null == clips)
            {
                return;
            }

            for (var index = 0; index < m_ChildMotions.Count; index++)
            {
                var clip = m_ChildMotions[index]?.clip;
                if (null != clip && !clips.Contains(clip))
                {
                    clips.Add(clip);
                }
            }
        }

        public virtual bool IsValidPlayable(AnimatorController ctrl)
        {
            if (animCtrl != ctrl || !m_IsValid)
            {
                return false;
            }

            for (var index = 0; index < m_ChildMotions.Count; index++)
            {
                if (!m_ChildMotions[index].isValid)
                {
                    return false;
                }
            }

            return true;
        }

        public override void RebuildPlayable(AnimatorController ctrl, AnimationMixerPlayable parent, int inputIndex)
        {
            m_IsValid = true;
            m_PlayableParent = parent;
            m_PlayableInputIndex = inputIndex;
            animCtrl = ctrl;
            mixerPlayable = AnimationMixerPlayable.Create(ctrl.playableGraph, childrenCount);

            for (var i = 0; i < childrenCount; i++)
            {
                m_ChildMotions[i].RebuildPlayable(ctrl, mixerPlayable, i);
            }

            var parameter = ctrl.GetParameter(m_BlendParameterName);
            if (null != parameter)
            {
                parameter.m_OnValueChanged -= OnParameterValueChanged;
                parameter.m_OnValueChanged += OnParameterValueChanged;
                SetChildWeight(parameter.defaultFloat);
            }
            else
            {
                SetChildWeight(0);
                // Debug.LogError($"[playable animator][frameCount:{Time.frameCount}][parameter not found, please check!!][parameterName:{m_BlendParameterName}]");
            }

            parent.DisconnectInput(inputIndex);
            parent.ConnectInput(inputIndex, mixerPlayable, 0, 1);
        }

        private void OnParameterValueChanged(AnimatorControllerParameter parameter)
        {
            if (null == parameter || null == animCtrl || parameter.animCtrl != animCtrl || m_BlendParameterName != parameter.name)
            {
                return;
            }

            if (!IsValidPlayable(animCtrl))
            {
                RebuildPlayable(animCtrl, m_PlayableParent, m_PlayableInputIndex);
            }

            SetChildWeight(parameter.defaultFloat);
        }

        private void SetChildWeight(float threshold)
        {
            var value = Mathf.Clamp(threshold, m_MinThreshold, m_MaxThreshold);
            if (childrenCount == 1)
            {
                mixerPlayable.SetInputWeight(0, 1);
            }
            else if (childrenCount > 1)
            {
                for (var i = 0; i < childrenCount;)
                {
                    if (i < childrenCount - 1 && value >= m_ChildMotions[i].threshold && value <= m_ChildMotions[i + 1].threshold)
                    {
                        var weight = (value - m_ChildMotions[i].threshold) / (m_ChildMotions[i + 1].threshold - m_ChildMotions[i].threshold);
                        mixerPlayable.SetInputWeight(i, 1 - weight);
                        mixerPlayable.SetInputWeight(i + 1, weight);
                        i += 2;
                    }
                    else
                    {
                        mixerPlayable.SetInputWeight(i, 0);
                        i += 1;
                    }
                }
            }

            m_Length = 0f;
            for (var i = 0; i < childrenCount; i++)
            {
                m_Length += mixerPlayable.GetInputWeight(i) * m_ChildMotions[i].length;
            }
        }
    }
}
