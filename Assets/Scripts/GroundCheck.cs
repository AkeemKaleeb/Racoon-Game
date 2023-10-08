using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded = false;

    private void OnTriggerEnter2D()
    {
        isGrounded = true;
    }

    private void OnTriggerExit2D()
    {
        isGrounded = false;
    }
}
