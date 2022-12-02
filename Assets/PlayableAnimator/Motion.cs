using System;
using UnityEngine.Animations;

namespace X3.PlayableAnimator
{
    [Serializable]
    public abstract class Motion
    {
        public IConcurrent concurrent { get; private set; }
        public abstract bool isLooping { get; }
        public abstract float length { get; }
        public abstract double normalizedTime { get; }
        public abstract void Reset();
        public abstract Motion DeepCopy();
        public abstract void RebuildPlayable(AnimatorController ctrl, AnimationMixerPlayable parent, int inputIndex);

        public virtual void OnPrepEnter(Motion prevMotion)
        {
            concurrent?.OnPrepEnter();
        }

        public virtual void OnEnter(Motion prevMotion)
        {
            concurrent?.OnEnter();
        }

        public virtual void OnPrepExit(Motion nextMotion)
        {
            concurrent?.OnPrepExit();
        }

        public virtual void OnExit(Motion nextMotion)
        {
            concurrent?.OnExit();
        }

        public virtual void OnDestroy()
        {
            concurrent?.OnDestroy();
        }

        public virtual void SetTime(double time)
        {
            concurrent?.SetTime(time);
        }

        public virtual void SetWeight(float weight)
        {
            concurrent?.SetWeight(weight);
        }

        public virtual void SetConcurrent(IConcurrent concurrent)
        {
            this.concurrent = concurrent;
        }
    }
}