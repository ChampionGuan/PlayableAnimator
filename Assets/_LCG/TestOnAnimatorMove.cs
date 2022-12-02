using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOnAnimatorMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 deltaPos;
    public Animator animator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAnimatorMove()
    {
        deltaPos = animator.deltaPosition;
    }
}
