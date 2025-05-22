using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseFighter : MonoBehaviour
{
    public FighterActions fighter_actions;
    public new Rigidbody2D rigidbody;
    public new Collider2D collider;

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
        fighter_actions.init();

        available_air_jumps = stats.air_jumps;

        // rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var dir = fighter_actions.direction.ReadValue<Vector2>();

        rigidbody.linearVelocityX = dir.x * 4.0f;

        if(fighter_actions.jump.WasPressedThisFrame())
        {
            if (grounded)
                jump();
            else if(available_air_jumps > 0)
            {
                jump();
                available_air_jumps--;
            }
        }
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
}
