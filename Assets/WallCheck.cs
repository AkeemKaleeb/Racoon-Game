using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public bool isWalled = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        isWalled = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        isWalled = false;
    }
}
