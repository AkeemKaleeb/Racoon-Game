using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public float jumpStrength = 15;
    public int numJumps = 1;

    Rigidbody2D rb;
    [SerializeField] WallCheck leftCheck;
    [SerializeField] WallCheck rightCheck;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void JumpUpdate()
    {
        if (leftCheck.isWalled || rightCheck.isWalled)
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
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        numJumps = 1;
    }
}