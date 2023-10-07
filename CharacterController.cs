using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    public float speed;
    public float jumpStrength;
    float moveVelocity;
    public int numJumps = 1;
    public int numDash = 1;
    public int dashDistance = 10;
    public Rigidbody2D rb;
    
    public void HandleUpdate()
    {
        //jumping?
        if (Input.GetButtonDown("Jump"))
        {
            //Able to Jump?
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

        //Left Right Movement
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            moveVelocity = -speed;
        }
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            moveVelocity = speed;
        }

        if (Input.GetButtonDown("Fire1") && numDash > 0)
        {
            rb.AddForce(rb.velocity.normalized * dashDistance);
        }

        rb.velocity = new Vector2(moveVelocity, rb.velocity.y);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        numJumps = 2;
    }
}


    

