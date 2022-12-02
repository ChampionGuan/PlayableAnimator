﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace X3.PlayableAnimator
{
    [Serializable]
    public abstract class State
    {
        [HideInInspector] [SerializeField] protected Vector2 m_Position;
        [SerializeField] protected string m_Name;
        [SerializeField] protected string m_Tag;
        [SerializeField] protected List<Transition> m_Transitions;

        public string name => m_Name;
        public string tag => m_Tag;
        public List<Transition> transitions => m_Transitions;
        public int transitionsCount => m_Transitions?.Count ?? 0;

        public State(Vector2 position, string name, string tag)
        {
            m_Name = name;
            m_Tag = tag;
            m_Position = position;
        }

        public virtual void Reset()
        {
        }

        public void AddTransition(List<Transition> transitions)
        {
            if (null == transitions)
            {
                return;
            }

            for (var index = 0; index < transitions.Count; index++)
            {
                AddTransition(transitions[index]);
            }
        }

        public void AddTransition(Transition transition)
        {
            if (null == transition)
            {
                return;
            }

            if (null == m_Transitions)
            {
                m_Transitions = new List<Transition>();
            }
            else if (m_Transitions.Contains(transition))
            {
                return;
            }

            m_Transitions.Add(transition);
        }

        public Transition GetTransition(int index)
        {
            if (null == m_Transitions || index < 0 || index > transitionsCount)
            {
                return null;
            }

            return m_Transitions[index];
        }

        public Transition GetTransition(string destStateName)
        {
            for (var index = 0; index < m_Transitions.Count; index++)
            {
                if (m_Transitions[index].destinationStateName == destStateName) return m_Transitions[index];
            }

            return null;
        }

        public void RemoveTransition(Transition transition)
        {
            if (null == transition || null == m_Transitions)
            {
                return;
            }

            m_Transitions.Remove(transition);
        }

        public void RemoveTransition(string stateName)
        {
            for (var i = transitionsCount - 1; i >= 0; i--)
            {
                if (m_Transitions[i].destinationStateName == stateName)
                {
                    m_Transitions.Remove(m_Transitions[i]);
                }
            }
        }

        public bool HasTransition(string destStateName)
        {
            for (var i = 0; i < transitionsCount; i++)
            {
                if (m_Transitions[i].destinationStateName == destStateName) return true;
            }

            return false;
        }

        public bool HasFadeTransition(string destStateName, out float fadeTime, out TransitionInterruptionSource interruptionSource)
        {
            interruptionSource = TransitionInterruptionSource.None;
            for (var i = transitionsCount - 1; i >= 0; i--)
            {
                if (m_Transitions[i].IsForDestinationStateFade(destStateName, out fadeTime, out interruptionSource))
                {
                    return true;
                }
            }

            fadeTime = 0;
            return false;
        }

        public void ClearTransition()
        {
            m_Transitions?.Clear();
        }
    }
}