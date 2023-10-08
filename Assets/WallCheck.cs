using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public bool isWalled = false;

    void OnTriggerEnter2D()
    {
        isWalled = true;
    }

    private void OnTriggerExit2D()
    {
        isWalled = false;
    }
}
