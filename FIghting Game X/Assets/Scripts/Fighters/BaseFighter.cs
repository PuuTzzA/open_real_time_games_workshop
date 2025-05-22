using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseFighter : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    private Vector2 moveAmount;
    private InputActionAsset InputActions;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void Awake()
    {
        rigidbody = this.GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.linearVelocityX = moveAmount.x * 4.0f;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveAmount = ctx.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        rigidbody.linearVelocityY = 5.0f;
    }
    
    
    public void OnHeavy(InputAction.CallbackContext ctx)
    {
        // Implement special move logic here
    }

}
