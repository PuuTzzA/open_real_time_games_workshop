using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class BaseFighter : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public Collider2D[] colliders;
    public Collider2D current_collider;
    public Collider2D hurtbox;

    public TextMeshPro debug_text;

    public FighterInput fighter_input;
    public EventBuffer event_buffer;

    public FighterState state;
    public FighterHealth health;

    private DelayedActions delayed_actions;

    private PlayerSounds player_sounds;

    // public MaterialSelector material_selector;

    private Action<int>[] frame_callbacks;

    // Tuple with x and y position (not a transform)
    public Vector2 deathBounds = new Vector2(-15.0f, -8.0f);
    public bool died = false;
    public GameObject holdingBomb;

    public Color playerColor;

    private Material material; 

    private static int next_id = 0;

    public readonly int id = next_id++;


    private readonly float[] dash_curve = new float[] {
    0.5f, 0.75f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
    0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f,
    0.2f, 0.1f, 0.05f, 0.0f, 0.0f
};
    private readonly float[] short_dash_curve = new float[] {
    1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.9f, 0.8f, 0.7f, 0.6f,
    0.5f, 0.4f, 0.3f, 0.2f, 0.1f, 0.0f,
    0.0f, 0.0f, 0.0f, 0.0f
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
        frame_callbacks[(int)FighterAction.KnockedBackLight] = knockback_light_tick;
        frame_callbacks[(int)FighterAction.KnockedBackHeavy] = knockback_heavy_tick;
        frame_callbacks[(int)FighterAction.HeavySide] = heavy_side_tick;

        frame_callbacks[(int)FighterAction.Ult] = ult_tick;

        frame_callbacks[(int)FighterAction.Crouch] = crouch_tick;


        state.start_action(FighterAction.Idle);

        material = GetComponentInChildren<SpriteRenderer>().material;

        select_collider(0);
    }

    private void EnableUltIndicator()
    {
        material.SetFloat("_CanUlt", 1f);   
    }

    private void DisableUltIndicator()
    {
        material.SetFloat("_CanUlt", 0f);   
    }

    public void FixedUpdate()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        current_collider.GetContacts(contacts);

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

        if (state.can_ult())
        {
            EnableUltIndicator();
        }
        else
        {
            DisableUltIndicator();
        }

        rigidbody.gravityScale = 1.0f;

        freezeXY(state.flags_any_set(FighterFlags.FreezeX), state.flags_any_set(FighterFlags.FreezeY));

        state.set_facing(fighter_input.direction.x);

        state.sprite_transform.eulerAngles = Vector3.zero;

        if (!state.flags_any_set(FighterFlags.CustomMovement))
            process_movement();

        gameObject.layer = state.flags_any_set(FighterFlags.Phasing) ? 10 : 6;

        hurtbox.enabled = !state.flags_any_set(FighterFlags.Invincible);

        // material_selector.set_elastic(false);
        select_collider(0);

        frame_callbacks[(int)state.get_action()]?.Invoke(state.animation_handler.get_index());

        debug_text.SetText(state.get_action() + "\n" + health.GetCurrentHealth() + "/" + health.maxHealth);

        state.set_grounded(false);

        if (state.freeze_pos.Item1)
            rigidbody.linearVelocityX = 0.0f;
        if (state.freeze_pos.Item2)
        {
            rigidbody.linearVelocityY = 0.0f;
            rigidbody.gravityScale = 0.0f;
        }

        state.animation_handler.step();
    }

    private void checkDeath()
    {
        if (this.transform.position.x < deathBounds.x ||
            transform.position.x > (-1 * deathBounds.x) || transform.position.y < deathBounds.y ||
            transform.position.y > (-3f * deathBounds.y))
        {
            died = true;
            health.TakeDamage(1000, null);
        }
    }

    public void next_idle_action()
    {
        FighterAction next_action = FighterAction.Idle;
        if (state.is_grounded())
        {
            if (fighter_input.direction.x != 0)
                next_action = FighterAction.Running;
            else if (fighter_input.direction.y == -1)
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
            if(new_dir_x != 0 && state.get_action() != FighterAction.Running)
            {
                state.start_action(FighterAction.Running);
            }
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

    public void knockback_light(Vector2 direction)
    {
        player_sounds.PlayJabHit();
        state.start_action(FighterAction.KnockedBackLight);
        freezeXY(false, false);
        rigidbody.linearVelocity = direction * (0.6f + health.GetMissingHealthPortion() * 0.4f);
    }

    public void knockback_heavy(Vector2 direction, int duration)
    {
        player_sounds.PlayJabHit();
        state.start_action(FighterAction.KnockedBackHeavy);
        freezeXY(false, false);
        select_collider(1);
        state.knockback_duration = duration;
        rigidbody.linearVelocity = direction * (0.7f + health.GetMissingHealthPortion() * 1.7f);
    }

    public void stun(int duration, bool keep_momentum = false)
    {
        player_sounds.PlayJabHit();
        state.stun_old_momentum = keep_momentum ? rigidbody.linearVelocity : Vector2.zero;
        state.stun_duration = Math.Max(duration, state.stun_duration);
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
        try
        {
            health.TakeDamage(damage, attacker);

        } catch (Exception e)
        {

        }
    }



    public void freezeXY(bool x, bool y)
    {
        //if (x)
        //{
        //    rigidbody.linearVelocityX = 0.0f;
        //    rigidbody.constraints |= RigidbodyConstraints2D.FreezePositionX;
        //}
        //else
        //{
        //    rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        //}

        //if (y)
        //{
        //    rigidbody.gravityScale = 0.0f;
        //    rigidbody.linearVelocityY = 0.0f;
        //    rigidbody.constraints |= RigidbodyConstraints2D.FreezePositionY;
        //}
        //else
        //{
        //    rigidbody.gravityScale = 1.0f;
        //    rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        //}

        state.freeze_pos = (x, y);
    }



    public void dash_tick(int index)
    {
        state.dash_available = state.is_grounded();

        freezeXY(false, true);
        if (index >= dash_curve.Length) return;

        float speed = dash_curve[index] * state.base_stats.dash_factor * state.base_stats.ground_speed * state.get_facing_float();
        rigidbody.linearVelocityX = speed;
    }

    public void heavy_up_tick(int index)
    {
        state.heavy_available[0] = state.is_grounded();

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
        state.heavy_available[2] = state.is_grounded();

        if (index < 26)
        {
            freezeXY(true, true);
            return;
        }

        if (index == 26)
        {
            if (state.is_grounded())
            {
                state.animation_handler.set_frozen(false);
            }
            else
            {
                state.animation_handler.set_frozen(true);
                rigidbody.linearVelocityY = -20.0f;
            }
        }
    }
    public void heavy_side_tick(int index)
    {
        state.heavy_available[1] = state.is_grounded();

        if (index < 22 || index > 38) return; // only active during frames 26–50

        freezeXY(false, true);
        int dash_index = index - 22;
        //if (dash_index >= short_dash_curve.Length) return;


        float speed = short_dash_curve[dash_index] * state.base_stats.dash_factor * state.base_stats.ground_speed * state.get_facing_float() * 0.9f;
        rigidbody.linearVelocityX = speed;
    }


    public void stun_tick(int index)
    {
        freezeXY(true, true);
        state.stun_duration--;
        state.animation_handler.set_frozen(state.stun_duration > 0);

        if(state.stun_duration <= 0)
        {
            freezeXY(false, false);
            rigidbody.linearVelocity = state.stun_old_momentum;
        }
    }

    public void knockback_light_tick(int index)
    {

    }

    public void knockback_heavy_tick(int index)
    {
        select_collider(1);
        if (index == 3)
        {
            state.animation_handler.set_frozen(--state.knockback_duration > 0);
        }
        else
        {
            state.animation_handler.set_frozen(false);
        }
        state.force_facing(Math.Sign(-rigidbody.linearVelocityX));
        state.sprite_transform.eulerAngles = Vector3.forward * (float)(Math.Atan2(-rigidbody.linearVelocityY, -rigidbody.linearVelocityX * state.get_facing_float()) * state.get_facing_float() * FighterState.knockback_rotation_factors[index] * 180.0f / Math.PI);
        // material_selector.set_elastic(true);
    }



    public void ult_tick(int index)
    {
        if (index == 0)
        {
            state.hammer_base_transform.gameObject.SetActive(true);
            state.hammer_base_transform.eulerAngles = Vector3.zero;
            state.hammer_animation_handler.play(FighterAction.UltHammer);
            state.ult_index = 0;
            state.ult_hitbox.init();
        }

        if (state.flags_any_set(FighterFlags.CustomMovement))
        {
            rigidbody.gravityScale = 0.0f;

            select_collider(2);

            var target_speed = new Vector2(fighter_input.direction.x, fighter_input.direction.y).normalized * state.get_air_speed() * 0.7f;

            var dist = target_speed - rigidbody.linearVelocity;

            float delta = state.get_air_speed() * Time.fixedDeltaTime * 2.0f;

            if (dist.magnitude <= delta)
            {
                rigidbody.linearVelocity = target_speed;
            }
            else
            {
                rigidbody.linearVelocity += delta * dist.normalized;
            }

            //var vel = rigidbody.linearVelocity.x;
            //rigidbody.linearVelocityX -= Math.Sign(vel) * 2.0f * state.air_resistance * vel * vel * Time.fixedDeltaTime;

            //vel = rigidbody.linearVelocity.y;
            //rigidbody.linearVelocityY -= Math.Sign(vel) * 2.0f * state.air_resistance * vel * vel * Time.fixedDeltaTime;

            //rigidbody.linearVelocity += new Vector2(fighter_input.direction.x, fighter_input.direction.y).normalized * state.get_air_speed() * Time.fixedDeltaTime * 5.0f;
        }

        if (state.ult_index < state.ult_data.Length)
        {
            state.hammer_base_transform.eulerAngles = Vector3.back * state.get_facing_float() * state.ult_data[state.ult_index].angle;
            state.hammer_animation_handler.set_index(state.ult_data[state.ult_index].index);
            state.hammer_animation_handler.show();

            if (index == 124)
                state.animation_handler.set_frozen(true);

            state.ult_index++;
        }
        else
        {
            if(index == 125)
            {
                Debug.Log("ult finished");
                state.ult_hitbox.knockback_fighters();
            }

            state.animation_handler.set_frozen(false);

            state.hammer_animation_handler.step();

            if (state.hammer_animation_handler.is_finished())
            {
                state.hammer_base_transform.gameObject.SetActive(false);
            }
            else
            {
                state.hammer_animation_handler.show();
            }
        }

        if (index >= 158)
            state.reset_ult_points();

        //state.ult_hitbox.reduce_fighter_cooldowns();
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
            delayed_actions.push(new DelayedAction(jump, 5));
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

        GetComponentInChildren<AttackHitbox>().hit_fighters.Clear();
        state.start_action((FighterAction)((int)(FighterAction.JabSide) - input.direction.y));
        return true;
    }

    public bool heavy_action(EventData input)
    {
        if (!input.pressed) return true;

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        if(!state.heavy_available[1 - input.direction.y]) return false;

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

        GetComponentInChildren<AttackHitbox>().hit_fighters.Clear();
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

        if (!state.is_grounded() && !state.dash_available) return false;

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

        if (!state.flags_any_set(FighterFlags.Interruptable)) return false;

        if(!state.can_ult()) return false;

        state.force_facing(input.direction.x);
        state.start_action(FighterAction.Ult);

        // knockback(new Vector2(-15 * state.get_facing_float(), 15), 15);
        //stun(120);
        return true;
    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
            handle_contact(contact);
    }

    public void select_collider(int index)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        current_collider = colliders[index];
        current_collider.enabled = true;
    }

    public void handle_contact(ContactPoint2D contact)
    {
        if (contact.normal.y > 0.7f)
        {
            state.set_grounded(true);
        }
    }
}
