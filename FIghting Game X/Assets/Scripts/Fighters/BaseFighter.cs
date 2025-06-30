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

    private PlayerSounds player_sounds;
    public Animator animator;

    public SubRoutine dash_routine;
    public SubRoutine heavy_up_routine;
    public SubRoutine heavy_down_routine;

    // Tuple with x and y position (not a transform)
    public Vector2 deathBounds = new Vector2(-15.0f, -8.0f);

    private SubRoutine current_subroutine = null;
    public bool died = false;


    private readonly float[] dash_curve = new float[] {
    0.5f, 0.75f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
    0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f,
    0.2f, 0.1f, 0.05f, 0.0f, 0.0f
};

    private void Awake()
    {
        player_sounds = GetComponent<PlayerSounds>();

        event_buffer = fighter_input.event_buffer;

        event_buffer.register(EventType.Jump, jump_action);
        event_buffer.register(EventType.Jab, jab_action);
        event_buffer.register(EventType.Heavy, heavy_action);
        event_buffer.register(EventType.Interact, interact_action);
        event_buffer.register(EventType.Dash, dash_action);
        event_buffer.register(EventType.Block, block_action);
        event_buffer.register(EventType.Ult, ult_action);

        delayed_actions = new DelayedActions();

        dash_routine = new SubRoutine(dash_curve.Length, dash_tick);
        heavy_up_routine = new SubRoutine(25, heavy_up_tick);
        heavy_down_routine = new SubRoutine(180, heavy_down_tick);

        state.start_action(FighterAction.Idle);
    }

    public void FixedUpdate()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }

        if (!died)
        {
            checkDeath();
        }

        if(state.action_tick() || state.is_idle())
        {
            next_idle_action();
        }

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

        freezeXY(state.flags_any_set(FighterFlags.FreezeX), state.flags_any_set(FighterFlags.FreezeY));

        if (state.remaining_flying_frames > 0)
        {
            state.remaining_flying_frames--;
        }
        else
        {
            process_movement();
        }

        if (current_subroutine == null || !current_subroutine.tick()) current_subroutine = null;

        debug_text.SetText(state.get_action() + "\n" + health.GetCurrentHealth() + "/" + health.maxHealth);

        state.set_grounded(false);
    }

    private void checkDeath()
    {
        if (this.transform.position.x < deathBounds.x ||
            transform.position.x > (-1 * deathBounds.x) || transform.position.y < deathBounds.y ||
            transform.position.y > (-3f * deathBounds.y))
        {
            died = true;
            health.TakeArenaDamage(1000);
        }
    }

    public void next_idle_action()
    {
        FighterAction next_action = FighterAction.Idle;
        if(state.is_grounded())
        {
            if(fighter_input.direction.x != 0)
                next_action = FighterAction.Running;
        } else
        {
            next_action = FighterAction.Falling;
        }

        if(next_action != state.get_action())
        {
            state.start_action(next_action);
            state.action_tick();
        }
    }


    public void process_movement()
    {
        state.set_facing(fighter_input.direction.x);

        //if(state.is_dashing())
        //{
        //    rigidbody.linearVelocityX = state.get_dash_speed();
        //    return;
        //}

        if (state.is_grounded())
        {
            rigidbody.linearVelocityX = !state.flags_any_set(FighterFlags.CanMove) ? 0.0f : fighter_input.direction.x * state.get_ground_speed();
        }
        else
        {
            float dist = (!state.flags_any_set(FighterFlags.CanMove) ? 0.0f : fighter_input.direction.x) * state.get_air_speed() - rigidbody.linearVelocityX;

            float delta = (dist * 5.0f + 2.0f * Math.Sign(dist)) * Time.fixedDeltaTime;
            rigidbody.linearVelocityX += delta;
            rigidbody.linearVelocityX = Math.Clamp(rigidbody.linearVelocityX, -state.get_air_speed(), state.get_air_speed());
        }

        if (rigidbody.linearVelocityY < state.get_terminal_speed())
        {
            rigidbody.linearVelocityY = state.get_terminal_speed();
        }
    }

    public void jump()
    {
        if (!state.is_grounded())
        {
            state.available_air_jumps--;
        }
        rigidbody.linearVelocityY = state.get_jump_strength();
    }

    public void knockback(Vector2 direction)
    {
        Debug.Log("knockback");
        player_sounds.PlayJabHit();
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

    public void take_arena_damage(int damage)
    {
        health.TakeArenaDamage(damage);
    }


    public void start_subroutine(SubRoutine routine)
    {
        if (current_subroutine != null) return;

        routine.start();
        current_subroutine = routine;
    }



    public void freezeXY(bool x, bool y)
    {
        if (x)
        {
            rigidbody.linearVelocityX = 0.0f;
            rigidbody.constraints |= RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        }

        if (y)
        {
            rigidbody.gravityScale = 0.0f;
            rigidbody.linearVelocityY = 0.0f;
            rigidbody.constraints |= RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            rigidbody.gravityScale = 1.0f;
            rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
    }


    public bool dash_tick(int index)
    {
        if (index >= dash_curve.Length) return false;

        float speed = dash_curve[index] * state.dash_speed * state.get_facing_float();
        rigidbody.linearVelocityX = speed;

        return true;
    }


    public bool heavy_up_tick(int index)
    {
        if (index < 18)
        {
            freezeXY(true, true);
            return true;
        }

        if (index >= 18)
        {
            rigidbody.linearVelocity = new Vector2(3.0f * state.get_facing_float(), 15.0f);
            return true;
        }
        return true;
    }

    public bool heavy_down_tick(int index)
    {
        if (index <= 28)
        {
            freezeXY(true, true);
            return true;
        }
        else
        {
            if (state.is_grounded())
            {
                animator.speed = 1.0f;
                return false;
            }
            else
            {
                rigidbody.linearVelocityY = -32.0f;
                return true;
            }

        }
    }


    public bool jump_action()
    {
        if (state.can_jump())
        {
            player_sounds.PlayJump();
            state.start_action(FighterAction.Jump);
            //jump();
            delayed_actions.push(new DelayedAction(jump, 10));
            return true;
        }
        return false;
    }


    public bool jab_action(EventData input)
    {
        if (!input.pressed) return true;

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        state.force_facing(input.direction.x);
        player_sounds.PlayJab();
        state.start_action((FighterAction)((int)(FighterAction.JabSide) - input.direction.y));
        return true;
    }

    public bool heavy_action(EventData input)
    {
        if (!input.pressed) return true;

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        state.force_facing(input.direction.x);
        player_sounds.PlayHeavy();
        state.start_action((FighterAction)((int)(FighterAction.HeavySide) - input.direction.y));
        switch (input.direction.y)
        {
            case -1:
                start_subroutine(heavy_down_routine);
                break;
            case 1:
                start_subroutine(heavy_up_routine);
                break;
        }
        return true;
    }

    public bool interact_action(EventData input)
    {
        return true;
    }

    public bool dash_action(EventData input)
    {
        if (!input.pressed) return true;

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        state.force_facing(input.direction.x);
        player_sounds.PlayDash();
        state.dash(state.base_stats.dash_factor * state.get_ground_speed());
        start_subroutine(dash_routine);

        return true;
    }

    public bool block_action(EventData input)
    {
        if (!input.pressed)
        {
            if (state.get_action() == FighterAction.BlockSide || state.get_action() == FighterAction.BlockUp)
                state.start_action(FighterAction.Idle);
            return true;
        }

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        state.force_facing(input.direction.x);
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
}
