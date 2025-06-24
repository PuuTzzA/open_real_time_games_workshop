using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor.VersionControl;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.Rendering.DebugUI;

public enum FighterAction
{
    Idle,
    Running,
    Jump,
    JabUp,
    JabSide,
    JabDown,
    Falling,
    Ult,
    Emote,
    BlockUp,
    BlockSide,
    HeavyUp,
    HeavySide,
    HeavyDown,
    Dash,
    KnockedBackLight,
    KnockedBackHeavy,
    Stunned
}

public enum Facing
{
    Left = -1,
    Right = 1
}

public class BaseFighter : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public new Collider2D collider;
    public SpriteRenderer sprite_renderer;
    public Transform sprite_transform;
    // public Animator animator;

    public TextMeshPro debug_text;

    public FighterInput fighter_input;
    public EventBuffer event_buffer;

    public FighterState state;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        event_buffer = fighter_input.event_buffer;

        event_buffer.register(EventType.Jump, jump_action);
        event_buffer.register(EventType.Jab, jab_action);
        event_buffer.register(EventType.Heavy, heavy_action);
        event_buffer.register(EventType.Interact, interact_action);
        event_buffer.register(EventType.Dash, dash_action);
        event_buffer.register(EventType.Block, block_action);
        event_buffer.register(EventType.Ult, ult_action);
    }

    public void FixedUpdate()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }

        check_animation_end();

        fighter_input.dispatch_events();

        event_buffer.process();

        /*
        if (remaining_dash_frames > 10)
        {
            rigidbody.gravityScale = 1.0f;
            rigidbody.linearVelocityX = 0.0f;
            // rigidbody.linearVelocityY = 0.0f;
            remaining_dash_frames--;
        }
        else /**/

        if (state.remaining_flying_frames > 0)
        {
            state.remaining_flying_frames--;
        }
        else if (state.remaining_dash_frames > 0)
        {
            rigidbody.gravityScale = 0.0f;
            rigidbody.linearVelocityX = state.dash_speed;
            rigidbody.linearVelocityY = 0.0f;
            state.remaining_dash_frames--;
        }
        else
        {
            rigidbody.gravityScale = 1.0f;
            process_movement();
        }

        debug_text.SetText(state.get_action().ToString());

        state.set_grounded(false);
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
            state.set_grounded(true);
        }
    }

    public void process_movement()
    {
        state.set_facing(fighter_input.direction.x);

        if (state.is_grounded())
        {
            rigidbody.linearVelocityX = !state.flags_any_set(FighterFlags.CanMove) ? 0.0f : fighter_input.direction.x * state.get_ground_speed();
        }
        else
        {
            float dist = (!state.flags_any_set(FighterFlags.CanMove) ? 0.0f : fighter_input.direction.x) * state.get_air_speed().x - rigidbody.linearVelocityX;

            float delta = (dist * 5.0f + 2.0f * Math.Sign(dist)) * Time.fixedDeltaTime;
            rigidbody.linearVelocityX += delta;
            rigidbody.linearVelocityX = Math.Clamp(rigidbody.linearVelocityX, -state.get_air_speed().x, state.get_air_speed().x);
        }
    }

    public void check_animation_end()
    {
        if (state.animation_data.finished)
        {
            state.animation_data.finished = false;
            on_animation_end();
        }
    }

    public void on_animation_end()
    {
        state.start_action(FighterAction.Idle);
    }

    public void jump()
    {
        rigidbody.linearVelocityY = state.get_jump_strength();
    }

    public void dash(float speed)
    {
        state.dash_speed = speed;
        state.remaining_dash_frames = 7;
    }

    public void knockback(Vector2 direction)
    {
        state.remaining_flying_frames = 4;
        rigidbody.linearVelocity = direction;
    }

    public bool jump_action()
    {
        if(state.can_jump())
        {
            jump();
            return true;
        }
        return false;
    }


    public bool jab_action(EventData input)
    {
        if (!input.pressed) return true;

        state.set_facing(input.direction.x);
        if (state.get_action() == FighterAction.JabSide) return false;

        state.start_action((FighterAction)((int)(FighterAction.JabSide) - input.direction.y));
        return true;
    }

    public bool heavy_action(EventData input)
    {
        if (!input.pressed) return true;

        state.set_facing(input.direction.x);
        state.start_action((FighterAction)((int)(FighterAction.HeavySide) - input.direction.y));
        return true;
    }

    public bool interact_action(EventData input)
    {
        return true;
    }

    public bool dash_action(EventData input)
    {
        if(!input.pressed) return true;
        state.set_facing(input.direction.x);
        dash((int)state.get_facing() * state.base_stats.dash_factor * state.get_ground_speed());
        return true;
    }

    public bool block_action(EventData input)
    {
        if (!input.pressed)
        {
            if (state.get_action() == FighterAction.BlockSide || state.get_action() == FighterAction.BlockUp)
                on_animation_end();
            return true;
        }

        state.set_facing(input.direction.x);
        state.start_action(input.direction.y == 1 ? FighterAction.BlockUp : FighterAction.BlockSide);
        return true;
    }

    public bool ult_action(EventData input)
    {
        if (!input.pressed) return true;

        state.start_action(FighterAction.KnockedBackLight);

        knockback(new Vector2(-(float)(int)state.get_facing(), 0.0f) * 5.0f);
        Debug.Log("knocking back");
        return true;
    }
}
