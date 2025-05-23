using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoalUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goalText;
    private bool goalShown = false;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnBtnPressed()
    {
        if (goalShown)
        {
            _animator.SetTrigger("hide");
            goalText.text = "Show Goal";
            goalShown = false;
        }
        else
        {
            _animator.SetTrigger("show");
            goalText.text = "Hide Goal";
            goalShown = true;
        }
    }
}
