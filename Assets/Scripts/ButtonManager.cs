using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private List<ButtonPress> buttons;
    public bool allPressed;

    private void Start()
    {
        buttons = new List<ButtonPress>();
        buttons = GetComponentsInChildren<ButtonPress>().ToList();
    }

    private void Update()
    {
        for(int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].isPressed == false)
            {
                break; 
            }
            if ( i == buttons.Count - 1 && buttons[buttons.Count - 1].isPressed)
            {
                allPressed = true;
            }
        }

    }
}
