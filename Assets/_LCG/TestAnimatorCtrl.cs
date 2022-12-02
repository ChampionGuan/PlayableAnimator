using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class TestAnimatorCtrl : MonoBehaviour
{
    public RuntimeAnimatorController m_ctrl;

    private PlayableGraph m_Graph;
    private AnimationPlayableOutput m_Output;
    private AnimationMixerPlayable m_Mixer;

    private AnimatorControllerPlayable m_ctrlPlayable;

    void Awake()
    {
        m_Graph = PlayableGraph.Create("Test_AnimatorCtrl_Graph");
        m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        // m_Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

        m_ctrlPlayable = AnimatorControllerPlayable.Create(m_Graph, m_ctrl);
        m_ctrlPlayable.Play("Default");

        m_Mixer = AnimationMixerPlayable.Create(m_Graph, 1);
        m_Mixer.ConnectInput(0, m_ctrlPlayable, 0, 1);

        m_Output = AnimationPlayableOutput.Create(m_Graph, name, GetComponent<UnityEngine.Animator>());
        m_Output.SetSourcePlayable(m_Mixer);
        m_Graph.Play();
        m_Graph.Evaluate();
    }

    private void Update()
    {
    }

    void LateUpdate()
    {
        Debug.Log(Time.frameCount + " : 111 : " + m_ctrlPlayable.GetTime() + "  ~~  " + m_ctrlPlayable.GetPreviousTime() + " ~~ " + Time.deltaTime + " ~~ " +
                  ((m_ctrlPlayable.GetPreviousTime() + Time.deltaTime) == m_ctrlPlayable.GetTime()));

        // m_testClip.m_clipPlayable.SetTime(m_ctrlPlayable.GetTime());
        // m_testClip.m_Graph.Evaluate();
    }
}