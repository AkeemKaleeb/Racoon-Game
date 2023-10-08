using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public float jumpStrength;
    public int numJumps = 1;
    public int numWallJumps = 1;
    public bool canWallJump;

    Rigidbody2D rb;
    [SerializeField] GroundCheck groundCheck;
    [SerializeField] WallCheck leftCheck;
    [SerializeField] WallCheck rightCheck;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void JumpUpdate()
    {
        if (groundCheck.isGrounded)
        {
            numJumps = 1;
            numWallJumps = 2;
        }

        if (leftCheck.isWalled || rightCheck.isWalled)
        {
            numJumps = 1;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (groundCheck.isGrounded && numJumps > 0 
                && (leftCheck.isWalled || rightCheck.isWalled))
            {
                numJumps--;
                rb.velocity = new Vector2(rb.velocity.x, jumpStrength);
            }

            if ((leftCheck.isWalled || rightCheck.isWalled) && canWallJump && numWallJumps > 0)
            {
                numWallJumps--;
                rb.velocity = new Vector2(jumpStrength * .7f, jumpStrength * .3f);
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .5f);
        }
    }
}
