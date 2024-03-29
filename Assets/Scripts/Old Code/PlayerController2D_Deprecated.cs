using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D_Deprecated : MonoBehaviour
{
    public int numDash = 1;
    public int dashDistance = 10;

    public bool playerCanMove = true;

    MoveState moveState = MoveState.Idle;
    public float speed = 12;
    float moveVelocity;

    Rigidbody2D rb;
    CharacterAnimator charAnim;
    Dash dash;
    Jump jump;

    SceneLoader sceneLoader;

    private void Start()
    {
        dash = GetComponent<Dash>();
        jump = GetComponent<Jump>();
        rb = GetComponent<Rigidbody2D>();
        charAnim = GetComponent<CharacterAnimator>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            sceneLoader.Reset();
        }

        if (Input.GetAxisRaw("Horizontal") > 0)
            moveState = MoveState.Right;
        else if (Input.GetAxisRaw("Horizontal") < 0)
            moveState = MoveState.Left;
        else
            moveState = MoveState.Idle;

        switch (moveState)
        {
            case MoveState.Idle:
                charAnim.IsMoving = false;
                moveVelocity = 0;
                break;
            case MoveState.Left:
                moveVelocity = -speed;
                charAnim.SetFacingDirection(FacingDirection.Left);
                charAnim.IsMoving = true;
                break;
            case MoveState.Right:
                moveVelocity = speed;
                charAnim.SetFacingDirection(FacingDirection.Right);
                charAnim.IsMoving = true;
                break;
        }

        rb.velocity = new Vector2(moveVelocity, rb.velocity.y);

        dash.DashUpdate();
        jump.JumpUpdate();
    }
}

public enum MoveState
{
    Left,
    Right,
    Idle
}