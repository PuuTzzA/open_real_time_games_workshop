using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BaseFighter : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public new Collider2D collider;

    public TextMeshPro debug_text;

    public Vector2 direction;
    public int discrete_x;

    public readonly FighterStats stats = FighterStats.DEFAULT;

    public int available_air_jumps;

    public FighterState state;

    public bool grounded
    {
        set
        {
            if (value)
            {
                available_air_jumps = stats.air_jumps;
            }
            state.grounded = value;
        }
        get { return state.grounded; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        available_air_jumps = stats.air_jumps;
        direction = Vector2.zero;
    }

    /*

    private Vector2 moveAmount;
    private InputActionAsset InputActions;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    */

    // Update is called once per frame
    void Update()
    {
        rigidbody.linearVelocityX = discrete_x * stats.ground_speed;

        debug_text.SetText(state.action.ToString());
        
    }

    public void FixedUpdate()
    {
        /*
        if (grounded)
        {
            rigidbody.linearVelocityX = discrete_x * stats.ground_speed;
        }
        else
        {
            float dist = discrete_x * stats.horizontal_speed - rigidbody.linearVelocityX;

            float delta = (dist * 4.0f + 2.0f * Math.Sign(dist)) * Time.fixedDeltaTime;
            rigidbody.linearVelocityX += delta;

            rigidbody.linearVelocityX = Math.Clamp(rigidbody.linearVelocityX, -stats.horizontal_speed, stats.horizontal_speed);
        }
        */

        grounded = false;

        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
            handle_contact(contact);
    }

    public void handle_contact(ContactPoint2D contact   )
    {
        if (contact.normal.y > 0.7f)
        {
            grounded = true;
        }
    }

    public void jump()
    {
        rigidbody.linearVelocityY = stats.jump_strength;
    }

    public void dash()
    {
        rigidbody.linearVelocityX = stats.ground_speed;
    }

    public void direction_action(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
        discrete_x = direction.x < -0.7f ? -1 : direction.x > 0.7f ? 1 : 0;
    }

    public void jump_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (grounded)
            jump();
        else if (available_air_jumps > 0)
        {
            jump();
            available_air_jumps--;
        }
    }


    public void jab_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }

    public void heavy_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }

    public void interact_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }

    public void dash_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        dash();
    }

    public void block_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }

    public void ult_action(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }
}
