using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    DashState dashState = DashState.Ready;
    Vector3 savedVelocity;
    Rigidbody2D rb;
    float dashTimer = 5;
    float maxDash = 5;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void DashUpdate()
    {
        switch (dashState)
        {
            case DashState.Ready:
                var isDashKeyDown = Input.GetButtonDown("Fire1");
                if (isDashKeyDown)
                {
                    savedVelocity = rb.velocity;
                    rb.velocity *= new Vector2(4f, 1.5f);
                    dashState = DashState.Dashing;
                }
                break;
            case DashState.Dashing:
                dashTimer += Time.deltaTime * 3;
                if (dashTimer >= maxDash)
                {
                    dashTimer = maxDash;
                    rb.velocity = savedVelocity;
                    dashState = DashState.Cooldown;
                }
                break;
            case DashState.Cooldown:
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0)
                {
                    dashTimer = 0;
                    dashState = DashState.Ready;
                }
                break;
        }
    }
}

public enum DashState
{
    Ready,
    Dashing,
    Cooldown
}