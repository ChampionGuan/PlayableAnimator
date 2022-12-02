using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StateListener : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("状态进入~~~~" + Time.frameCount + "--" + stateInfo.normalizedTime);
        // EditorApplication.isPaused = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Debug.Log("状态进入~~~~" + Time.frameCount + "--" + stateInfo.normalizedTime);
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}