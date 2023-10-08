using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public float jumpStrength = 15;
    public int numJumps = 1;
    public int numWallJumps = 1;

    Rigidbody2D rb;
    BoxCollider2D bc;
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
        else if (leftCheck.isWalled || rightCheck.isWalled)
        {
            numJumps = 1;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (numJumps > 0)
            {
                numJumps--;
                rb.velocity = new Vector2(rb.velocity.x, jumpStrength);
                
            } 
            else if (numWallJumps > 0)
            {
                if (leftCheck.isWalled)
                {
                    numWallJumps--;
                    rb.velocity = new Vector2(jumpStrength * .7f, jumpStrength);
                }
                else if (rightCheck.isWalled)
                {
                    numWallJumps--;
                    rb.velocity = new Vector2(jumpStrength * -.7f, jumpStrength);
                }
                
            }
        }
        else if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .5f);
        }
    }
}