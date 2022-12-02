using System.Collections.Generic;
using UnityEngine;

namespace X3.PlayableAnimator
{
    public class StatePose : StatePlayable
    {
        public AnimatorControllerLayer animLayer { get; }
        public override bool isValid { get; protected set; } = true;
        public override float internalWeight { get; protected set; }
        public override float speed => 1;
        public override bool isRunning => m_Status != InternalStatusType.Exit;
        public override bool isLooping => m_MainState.isLooping;
        public override float length => m_MainState.length;
        public override double normalizedTime => m_MainState.normalizedTime;
        public override Motion motion => null;

        private Pose m_Pose;
        private Pose m_CachePose;
        private StateMotion m_MainState;
        private StatePlayable m_NextState;
        private InternalStatusType m_Status = InternalStatusType.Exit;
        private double m_FadeOutTick;

        public StatePose(AnimatorControllerLayer layer) : base(Vector2.zero, null, null)
        {
            animLayer = layer;
        }

        public void Enter(StateMotion mainState, StatePlayable prevState, StatePlayable currState)
        {
            if (null == mainState)
            {
                return;
            }

            var pose = null == m_Pose ? PoseA.Get(currState, prevState) : PoseB.Get(mainState, m_Pose) as Pose;
            m_MainState = mainState;
            m_Name = mainState.name;
            m_Pose = pose;
            m_Status = InternalStatusType.PrepEnter;
        }

        private void Exit()
        {
            m_CachePose = m_Pose;
            m_Pose = null;
            m_CachePose.OnExit(m_NextState);
        }

        public override void OnEnter(float enterTime, StatePlayable prevState)
        {
        }

        public override void OnUpdate(float deltaTime, float weight)
        {
            if (!isRunning)
            {
                return;
            }

            SetWeight(weight);
            switch (m_Status)
            {
                case InternalStatusType.PrepExit:
                    m_MainState?.CheckToDestinationState();
                    break;
                case InternalStatusType.PrepEnter:
                    m_Status = InternalStatusType.Enter;
                    break;
            }

            if (m_Status == InternalStatusType.PrepExit && (m_FadeOutTick -= deltaTime) <= 0)
            {
                m_Status = InternalStatusType.Exit;
                Exit();
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
            if (m_Status == InternalStatusType.Exit)
            {
                Exit();
            }
        }

        public override void OnDestroy()
        {
        }

        public override void Reset()
        {
        }

        public override void SetWeight(float weight, bool syncToPlayable = true)
        {
            internalWeight = weight;
            if (syncToPlayable) m_Pose?.SetWeight(weight);
        }

        public override void SetSpeed(float speed)
        {
        }

        public bool ContainState(StatePlayable state)
        {
            if (null == state || null == m_Pose)
            {
                return false;
            }

            return m_Pose.ContainState(state);
        }

        public abstract class Pose
        {
            public float weight { get; protected set; }
            public abstract bool ContainState(StatePlayable state);
            public abstract void SetWeight(float weight);
            public abstract void OnExit(StatePlayable nextState);
        }

        public class PoseA : Pose
        {
            public StatePlayable stateA { get; protected set; }
            public StatePlayable stateB { get; protected set; }

            public override bool ContainState(StatePlayable state)
            {
                return stateA == state || stateB == state;
            }

            public override void SetWeight(float weight)
            {
                stateA.SetWeight(weight * this.weight);
                stateB.SetWeight(weight * (1 - this.weight));
            }

            public override void OnExit(StatePlayable nextState)
            {
                stateB?.OnExit(0, nextState);
                stateA?.OnExit(0, nextState);
                Back(this);
            }


            private static Stack<PoseA> _idle = new Stack<PoseA>();

            public static void Back(PoseA pose)
            {
                if (null == pose)
                {
                    return;
                }

                pose.weight = 0;
                pose.stateA = null;
                pose.stateB = null;
                _idle.Push(pose);
            }

            public static PoseA Get(StatePlayable stateA, StatePlayable stateB)
            {
                var pose = _idle.Count > 0 ? _idle.Pop() : new PoseA();
                pose.weight = stateA.internalWeight;
                pose.stateA = stateA;
                pose.stateB = stateB;
                return pose;
            }
        }

        public class PoseB : Pose
        {
            public StatePlayable stateA { get; protected set; }
            public Pose poseB { get; protected set; }

            public override bool ContainState(StatePlayable state)
            {
                return stateA == state || poseB.ContainState(state);
            }

            public override void SetWeight(float weight)
            {
                stateA.SetWeight(weight * this.weight);
                poseB.SetWeight(weight * (1 - this.weight));
            }

            public override void OnExit(StatePlayable nextState)
            {
                poseB.OnExit(nextState);
                stateA.OnExit(0, nextState);
                Back(this);
            }


            private static Stack<PoseB> _idle = new Stack<PoseB>();

            public static void Back(PoseB pose)
            {
                if (null == pose)
                {
                    return;
                }

                pose.weight = 0;
                pose.stateA = null;
                pose.poseB = null;
                _idle.Push(pose);
            }

            public static PoseB Get(StatePlayable stateA, Pose poseB)
            {
                var pose = _idle.Count > 0 ? _idle.Pop() : new PoseB();
                pose.weight = stateA.internalWeight;
                pose.stateA = stateA;
                pose.poseB = poseB;
                return pose;
            }
        }
    }
}
