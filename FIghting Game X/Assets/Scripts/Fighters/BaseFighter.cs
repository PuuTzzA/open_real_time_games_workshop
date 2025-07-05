using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor.VersionControl;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.Rendering.DebugUI;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;

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

    private Action<int>[] frame_callbacks;

    // Tuple with x and y position (not a transform)
    public Vector2 deathBounds = new Vector2(-15.0f, -8.0f);
    public bool died = false;
    public GameObject holdingBomb;


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

        frame_callbacks = new Action<int>[Enum.GetValues(typeof(FighterAction)).Length];

        frame_callbacks[(int)FighterAction.Dash] = dash_tick;
        frame_callbacks[(int)FighterAction.HeavyUp] = heavy_up_tick;
        frame_callbacks[(int)FighterAction.HeavyDown] = heavy_down_tick;

        frame_callbacks[(int)FighterAction.Stunned] = stun_tick;

        frame_callbacks[(int)FighterAction.Crouch] = crouch_tick;

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

        if (state.animation_handler.is_finished() || state.is_idle())
        {
            next_idle_action();
        }

        state.animation_handler.show();


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

        frame_callbacks[(int)state.get_action()]?.Invoke(state.animation_handler.get_index());

        debug_text.SetText(state.get_action() + "\n" + health.GetCurrentHealth() + "/" + health.maxHealth);

        state.set_grounded(false);

        state.animation_handler.step();
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
        if (state.is_grounded())
        {
            if (fighter_input.direction.x != 0)
                next_action = FighterAction.Running;
            else if(fighter_input.direction.y == -1)
                next_action = FighterAction.Crouch;
        }
        else
        {
            next_action = FighterAction.Falling;
        }

        if (next_action != state.get_action())
        {
            state.start_action(next_action);
        }
    }


    public void process_movement()
    {
        state.set_facing(fighter_input.direction.x);

        // air resistance
        var vel = rigidbody.linearVelocity.y;
        if (vel < 0)
        {
            rigidbody.linearVelocityY += state.air_resistance * vel * vel * Time.fixedDeltaTime;
        }

        var new_dir_x = !state.flags_any_set(FighterFlags.CanMove) ? 0.0f : fighter_input.direction.x;

        if (state.is_grounded())
        {
            rigidbody.linearVelocityX = new_dir_x * state.get_ground_speed();
        }
        else
        {
            vel = rigidbody.linearVelocity.x;
            rigidbody.linearVelocityX -= Math.Sign(vel) * 2.0f * state.air_resistance * vel * vel * Time.fixedDeltaTime;

            float delta = new_dir_x * state.get_air_speed() * Time.fixedDeltaTime * 5.0f;

            rigidbody.linearVelocityX += delta;
            // rigidbody.linearVelocityX = Math.Clamp(rigidbody.linearVelocityX, -state.get_air_speed(), state.get_air_speed());
        }

        //if (rigidbody.linearVelocityY < state.get_terminal_speed())
        //{
        //    rigidbody.linearVelocityY = state.get_terminal_speed();
        //}
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
        player_sounds.PlayJabHit();
        state.start_action(FighterAction.KnockedBackLight);
        state.remaining_flying_frames = 5;
        rigidbody.linearVelocity = direction;
    }

    public void stun(int duration)
    {
        state.stun_duration = duration;
        state.start_action(FighterAction.Stunned);
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



    public void dash_tick(int index)
    {
        freezeXY(false, true);
        if (index >= dash_curve.Length) return;

        float speed = dash_curve[index] * state.base_stats.dash_factor * state.base_stats.ground_speed * state.get_facing_float();
        rigidbody.linearVelocityX = speed;
    }

    public void heavy_up_tick(int index)
    {
        if (index < 18)
        {
            freezeXY(true, true);
            return;
        }

        if (index >= 18 && index < 26)
        {
            rigidbody.linearVelocity = new Vector2(3.0f * state.get_facing_float(), 15.0f);
            return;
        }
    }

    public void heavy_down_tick(int index)
    {
        if (index < 26)
        {
            freezeXY(true, true);
            return;
        }

        if(index == 26)
        {
            if(state.is_grounded())
            {
                state.animation_handler.set_frozen(false);
            } else
            {
                state.animation_handler.set_frozen(true);
                rigidbody.linearVelocityY = -32.0f;
            }
        }
    }

    public void stun_tick(int index)
    {
        state.animation_handler.set_frozen(--state.stun_duration > 0);
    }

    public void crouch_tick(int index)
    {
        next_idle_action();
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
        switch (input.direction.y)
        {
            case -1:
                player_sounds.PlayHeavySidewaysClip();
                break;
            case 0:
                player_sounds.PlayHeavySidewaysClip();
                break;
            case 1:
                player_sounds.PlayHeavyUpClip();
                break;
        }
        // player_sounds.PlayHeavySidewaysClip();
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

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        state.force_facing(input.direction.x);
        player_sounds.PlayDash();
        state.start_action(FighterAction.Dash);

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

        stun(180);
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
