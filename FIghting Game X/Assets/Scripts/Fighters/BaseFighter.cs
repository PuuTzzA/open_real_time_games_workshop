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

    public FighterInput fighter_input;
    public ActionBuffer action_buffer;

    public readonly FighterStats stats = FighterStats.DEFAULT;

    public int available_air_jumps;
    public int remaining_dash_frames;

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

        remaining_dash_frames = 0;

        action_buffer = new ActionBuffer();

        action_buffer.register(FighterButton.Jump, jump_action);
        action_buffer.register(FighterButton.Jab, jab_action);
        action_buffer.register(FighterButton.Heavy, heavy_action);
        action_buffer.register(FighterButton.Interact, interact_action);
        action_buffer.register(FighterButton.Dash, dash_action);
        action_buffer.register(FighterButton.Block, block_action);
        action_buffer.register(FighterButton.Ult, ult_action);

        state = FighterState.create();
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

    public void FixedUpdate()
    {
        fighter_input.dispatch_events(action_buffer);

        action_buffer.process();

        
        /*
        if (remaining_dash_frames > 10)
        {
            rigidbody.gravityScale = 1.0f;
            rigidbody.linearVelocityX = 0.0f;
            // rigidbody.linearVelocityY = 0.0f;
            remaining_dash_frames--;
        }
        else /**/
        if (remaining_dash_frames > 0)
        {
            rigidbody.gravityScale = 0.0f;
            rigidbody.linearVelocityX = (int)state.facing * stats.ground_speed * 3.0f;
            rigidbody.linearVelocityY = 0.0f;
            remaining_dash_frames--;
        }
        else
        {
            rigidbody.gravityScale = 1.0f;
            if (fighter_input.direction.x != 0)
                state.facing = (Facing)fighter_input.direction.x;
            rigidbody.linearVelocityX = fighter_input.direction.x * stats.ground_speed;
        }
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

        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }

        debug_text.SetText(state.action.ToString());

        grounded = false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
            handle_contact(contact);
    }

    public void handle_contact(ContactPoint2D contact)
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
        remaining_dash_frames = 7;
    }

    public bool jump_action()
    {
        if (grounded)
        {
            jump();
            return true;
        }
        else if (available_air_jumps > 0)
        {
            jump();
            available_air_jumps--;
            return true;
        }
        return false;
    }


    public bool jab_action()
    {
        state.action = CurrentAction.Jab;
        return true;
    }

    public bool heavy_action(ActionInput input)
    {
        state.action = CurrentAction.Heavy;
        return true;
    }

    public bool interact_action(ActionInput input)
    {
        return true;
    }

    public bool dash_action()
    {
        dash();
        return true;
    }

    public bool block_action(ActionInput input)
    {
        state.action = input.pressed ? CurrentAction.Block : CurrentAction.NoAction;
        return true;
    }

    public bool ult_action(ActionInput input)
    {
        state.action = CurrentAction.Ult;
        return true;
    }
}
