using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using X3.PlayableAnimator;

public class TestAnimCtrl : MonoBehaviour
{
    public PlayableAnimator _animCtrl;
    public Animator animator;


    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _animCtrl.SetFloat("test", 60);
            animator.SetFloat("test", 60);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            _animCtrl.SetFloat("test", 110);
            animator.SetFloat("test", 110);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            _animCtrl.SetFloat("test", 160);
            animator.SetFloat("test", 160);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            _animCtrl.SetFloat("test", 60);
            animator.SetFloat("test", 60);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            _animCtrl.SetFloat("test22", 180);
            animator.SetFloat("test22", 180);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _animCtrl.SetFloat("test22", 20);
            animator.SetFloat("test22", 20);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // _animCtrl.SetFloat("test", 0);
            // _animCtrl.SetFloat("test22", 0);
            //
            // animator.SetFloat("test", 0);
            // animator.SetFloat("test22", 0);

            _animCtrl.Play("RunStart", 0, 0);
            animator.Play("RunStart", 0, 0);
        }
    }
}
