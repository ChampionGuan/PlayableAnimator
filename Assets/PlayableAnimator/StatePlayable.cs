using System;
using UnityEngine;
using UnityEngine.Animations;

namespace X3.PlayableAnimator
{
    public abstract class StatePlayable : State
    {
        public abstract float internalWeight { get; protected set; }
        public abstract bool isValid { get; protected set; }
        public abstract float speed { get; }
        public abstract bool isRunning { get; }
        public abstract bool isLooping { get; }
        public abstract float length { get; }
        public abstract double normalizedTime { get; }
        public abstract Motion motion { get; }

        public abstract void OnEnter(float enterTime, StatePlayable prevState);
        public abstract void OnUpdate(float deltaTime, float weight);
        public abstract void OnExit(float fadeOutDuration, StatePlayable nextState);
        public abstract void OnDestroy();
        public abstract void SetWeight(float weight, bool syncToPlayable = true);
        public abstract void SetSpeed(float speed);

        protected StatePlayable(Vector2 position, string name, string tag) : base(position, name, tag)
        {
        }

        protected enum InternalStatusType
        {
            Exit = 0,
            PrepExit,
            Enter,
            PrepEnter,
        }
    }
}
