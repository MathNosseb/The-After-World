using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public event Action<Vector3> OnMouseMove;
    public event Action<Vector3> OnMove;
    public event Action<bool> OnJump;
    public event Action OnInteract;
    public event Action OnPadUp;
    public event Action OnPadDown;

    private void Update()
    {

        //Mouse
        float rollInput = (((Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton4))? 1f : 0f) - ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton5))? 1f : 0f));
        Vector3 mouse = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), rollInput);
        OnMouseMove?.Invoke(mouse);//declenche l'event si si qqn est abonn�e

        //Move
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        OnMove?.Invoke(moveDirection);//on invoque meme si on bouge pas car il y a un Slerp et des valeurs � 0

        //Jump
        bool jumping;
        if (Input.GetButton("Jump") || Input.GetKey(KeyCode.JoystickButton0))
        {
            jumping = true;
        }
        else
        {
            jumping = false;
        }
        OnJump?.Invoke(jumping);

        //Interact
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
            OnInteract?.Invoke();

        //up
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // appui unique
            OnPadUp?.Invoke();
        }
        //down
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // appui unique
            OnPadDown?.Invoke();
        }

        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + i)))
            {
                Debug.Log("Bouton " + i);
            }
        }
    }
}
