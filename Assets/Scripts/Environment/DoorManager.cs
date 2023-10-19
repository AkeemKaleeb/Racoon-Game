using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] private GameObject door;

    private void Start()
    {
        buttonManager = FindObjectOfType<ButtonManager>();
        if(door.GetComponent<SpriteRenderer>() == null)
        {
            door.AddComponent<SpriteRenderer>();
        }
        if(door.GetComponent<BoxCollider2D>() == null)
        {
            door.AddComponent<BoxCollider2D>();
        }

    }

    private void Update()
    {
        if (buttonManager.allPressed)
        {
            door.GetComponent<BoxCollider2D>().enabled = false;
            door.GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            door.GetComponent<BoxCollider2D>().enabled = true;
            door.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
