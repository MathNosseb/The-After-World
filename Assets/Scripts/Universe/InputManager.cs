using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public event Action<Vector3> OnMouseMove;
    public event Action<Vector3> OnMove;
    public event Action<bool> OnJump;
    public event Action OnInteract;

    private void Update()
    {
        //Mouse
        float rollInput = ((Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f)) * Time.deltaTime;
        Vector3 mouse = new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime, Input.GetAxisRaw("Mouse Y") * Time.deltaTime, rollInput).normalized;
        OnMouseMove?.Invoke(mouse);//declenche l'event si si qqn est abonnée

        //Move
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        OnMove?.Invoke(moveDirection);//on invoque meme si on bouge pas car il y a un Slerp et des valeurs à 0

        //Jump
        bool jumping = Input.GetButton("Jump");
        OnJump?.Invoke(jumping);

        //Interact
        if (Input.GetKeyDown(KeyCode.F))
            OnInteract?.Invoke();
    }
}
