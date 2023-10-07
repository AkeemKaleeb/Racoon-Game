using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }
}
