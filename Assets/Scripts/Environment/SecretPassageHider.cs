using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretPassageHider : MonoBehaviour
{
    SpriteRenderer sr;
    public float _revealSpeed;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (sr.color.a > 0)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a - Time.deltaTime * _revealSpeed);
        }
    }
}
