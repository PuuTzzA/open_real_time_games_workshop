using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor.VersionControl;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.Rendering.DebugUI;
using System.Runtime.CompilerServices;

public class BaseFighter : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public new Collider2D collider;

    public TextMeshPro debug_text;

    public FighterInput fighter_input;
    public EventBuffer event_buffer;

    public FighterState state;
    public FighterHealth health;

    private DelayedActions delayed_actions;


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

        delayed_actions = new DelayedActions();
    }

    public void FixedUpdate()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }

        check_signals();

        delayed_actions.tick();

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

        debug_text.SetText(state.get_action() + "\n" + health.GetCurrentHealth() + "/" + health.maxHealth);

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

    public void check_signals()
    {
        var signals = state.read_signals();

        if (signals.HasFlag(FighterSignals.Finished))
        {
            on_animation_end();
        }

        if (signals.HasFlag(FighterSignals.ShouldJump))
        {
            //jump();
        }
    }

    public void on_animation_end()
    {
        state.start_action(FighterAction.Idle);
        state.animation_data.flags = FighterFlags.Idle;
    }

    public void jump()
    {
        if (!state.is_grounded())
        {
            state.available_air_jumps--;
        }
        rigidbody.linearVelocityY = state.get_jump_strength();
    }

    public void dash(float speed)
    {
        state.dash_speed = speed;
        state.remaining_dash_frames = 7;
    }

    public void knockback(Vector2 direction)
    {
        Debug.Log("knockback");
        state.start_action(FighterAction.KnockedBackLight);
        state.remaining_flying_frames = 5;
        rigidbody.linearVelocity = direction;
    }

    public void handle_hit(AttackHitbox hitbox_data)
    {
        Debug.Log(hitbox_data.knockback);
        Debug.Log(hitbox_data.source_fighter.state.get_facing_vec());
        knockback(hitbox_data.knockback * hitbox_data.source_fighter.state.get_facing_vec());
    }

    public bool is_blocking(Vector2Int direction)
    {
        if (state.flags_any_set(FighterFlags.BlockSide) && direction.x == -state.get_facing_int())
            return true;

        if (state.flags_any_set(FighterFlags.BlockUp) && direction.y == -1)
            return true;

        return false;
    }


    public void take_damage(int damage, GameObject attacker)
    {
        health.TakeDamage(damage, attacker);
    }




    public bool jump_action()
    {
        if (state.can_jump())
        {
            state.start_action(FighterAction.Jump);
            delayed_actions.push(new DelayedAction(jump, 8));
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
        if (!input.pressed) return true;
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

        knockback(new Vector2(-(float)(int)state.get_facing(), 0.0f) * 5.0f);
        Debug.Log("knocking back");
        return true;
    }
}
