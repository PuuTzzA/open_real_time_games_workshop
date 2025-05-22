using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseFighter : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public new Collider2D collider;

    public Vector2 direction;

    public readonly FighterStats stats = FighterStats.DEFAULT;

    public int available_air_jumps;

    private bool _grounded;
    public bool grounded
    {
        set
        {
            if (value)
            {
                available_air_jumps = stats.air_jumps;
            }
            _grounded = value;
        }
        get { return _grounded; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        available_air_jumps = stats.air_jumps;
        direction = Vector2.zero;
    }

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

    // Update is called once per frame
    void Update()
    {
        rigidbody.linearVelocityX = direction.x * 4.0f;
    }

    public void FixedUpdate()
    {
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

    public void direction_action(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
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
}
