using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public event Action<Vector2> OnMouseMove;
    public event Action<Vector3> OnMove;
    public event Action OnJump;

    private void Update()
    {
        //Mouse
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        OnMouseMove?.Invoke(mouse);//declenche l'event si si qqn est abonnée

        //Move
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        OnMove?.Invoke(moveDirection);//on invoque meme si on bouge pas car il y a un Slerp et des valeurs à 0

        //Jump
        if (Input.GetButton("Jump"))
            OnJump?.Invoke();
    }
}
