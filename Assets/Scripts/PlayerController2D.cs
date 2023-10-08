using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    
    public float jumpStrength;
    public int numJumps = 1;
    public int numWallJumps = 1;
    public int numDash = 1;
    public int dashDistance = 10;    

    MoveState moveState = MoveState.Idle;
    public float speed = 12;
    float moveVelocity;

    DashState dashState = DashState.Ready;
    public float dashTimer;
    public float maxDash = 20f;
    public Vector2 savedVelocity;

    Rigidbody2D rb;
    [SerializeField] GroundCheck groundCheck;
    [SerializeField] WallCheck leftCheck;
    [SerializeField] WallCheck rightCheck;
    CharacterAnimator charAnim;

    SceneLoader sceneLoader;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        charAnim = GetComponent<CharacterAnimator>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            sceneLoader.Reset();
        }

        if (groundCheck.isGrounded)
        {
            numJumps = 1;
        }

        if (leftCheck.isWalled || rightCheck.isWalled)
        {
            numWallJumps = 1;
            numJumps = 1;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (leftCheck.isWalled || rightCheck.isWalled)
            {
                numWallJumps -= 1;
                rb.velocity = new Vector2(jumpStrength * Mathf.Cos(1 / 2), jumpStrength * Mathf.Sin(Mathf.Pow(2, 1 / 2) / 2));
            }

            if (numJumps > 0)
            {
                numJumps -= 1;
                rb.velocity = new Vector2(rb.velocity.x, jumpStrength);                
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .5f);
        }

        moveVelocity = 0;

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

public enum MoveState
{
    Left,
    Right,
    Idle
}

