using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ButtonPress : MonoBehaviour
{
    public bool isPressed = false;
    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(isPressed)
        {
            anim.Play("pressed");
        }
        else
        {
            anim.Play("unpressed");
        }
    }

    private void OnTriggerEnter2D()
    {
        isPressed = true;
    }

    private void OnTriggerExit2D()
    {
        isPressed = false;
    }
}
