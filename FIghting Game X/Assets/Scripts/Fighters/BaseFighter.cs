using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor.VersionControl;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

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
    public Animator animator;

    public TextMeshPro debug_text;

    public FighterInput fighter_input;
    public EventBuffer event_buffer;

    public readonly BaseStats base_stats = BaseStats.DEFAULT;

    public int available_air_jumps;
    public int remaining_dash_frames;
    public int remaining_flying_frames;

    private FighterAction _current_action;

    public void start_action(FighterAction action)
    {
        if (_current_action != action)
        {
            _current_action = action;

            animator.ResetTrigger("trigger");
            animator.SetInteger("action", (int)action);
            animator.SetTrigger("trigger");
        }
    }
    public FighterAction get_action() { return _current_action; }

    private bool _grounded;
    private Facing _facing;

    public bool passive = true;

    public AnimationData animation_data;

    public bool grounded
    {
        set
        {
            if (value)
            {
                available_air_jumps = base_stats.air_jumps;
            }
            _grounded = value;
        }
        get { return _grounded; }
    }

    public Facing facing
    {
        set
        {
            if ((animation_data.flags & FighterFlags.CanTurn) == 0) return;
            _facing = value;

            var scale = sprite_transform.localScale;
            scale.x = (int)value;
            sprite_transform.localScale = scale;
        }
        get { return _facing; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        available_air_jumps = base_stats.air_jumps;
        facing = Facing.Right;
        remaining_dash_frames = 0;

        event_buffer = new EventBuffer();

        event_buffer.register(FighterButton.Jump, jump_action);
        event_buffer.register(FighterButton.Jab, jab_action);
        event_buffer.register(FighterButton.Heavy, heavy_action);
        event_buffer.register(FighterButton.Interact, interact_action);
        event_buffer.register(FighterButton.Dash, dash_action);
        event_buffer.register(FighterButton.Block, block_action);
        event_buffer.register(FighterButton.Ult, ult_action);

        start_action(FighterAction.Idle);
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
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        collider.GetContacts(contacts);

        foreach (var contact in contacts)
        {
            handle_contact(contact);
        }

        check_animation_end();

        fighter_input.dispatch_events(event_buffer);

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

        if (remaining_flying_frames > 0)
        {
            remaining_flying_frames--;
        }
        else if (remaining_dash_frames > 0)
        {
            rigidbody.gravityScale = 0.0f;
            rigidbody.linearVelocityX = (int)facing * base_stats.ground_speed * 3.0f;
            rigidbody.linearVelocityY = 0.0f;
            remaining_dash_frames--;
        }
        else
        {
            rigidbody.gravityScale = 1.0f;
            process_movement();
        }

        debug_text.SetText(get_action().ToString());

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

    public void process_movement()
    {
        if (fighter_input.direction.x != 0)
            facing = (Facing)fighter_input.direction.x;

        if (grounded)
        {
            rigidbody.linearVelocityX = ((animation_data.flags & FighterFlags.CanMove) == 0) ? 0.0f : fighter_input.direction.x * base_stats.ground_speed;
        }
        else
        {
            float dist = ((animation_data.flags & FighterFlags.CanMove) == 0 ? 0.0f : fighter_input.direction.x) * base_stats.horizontal_speed - rigidbody.linearVelocityX;

            float delta = (dist * 5.0f + 2.0f * Math.Sign(dist)) * Time.fixedDeltaTime;
            rigidbody.linearVelocityX += delta;
            rigidbody.linearVelocityX = Math.Clamp(rigidbody.linearVelocityX, -base_stats.horizontal_speed, base_stats.horizontal_speed);
        }
    }

    public void check_animation_end()
    {
        if (animation_data.finished)
        {
            animation_data.finished = false;
            on_animation_end();
        }
    }

    public void on_animation_end()
    {
        passive = true;

        start_action(FighterAction.Idle);
    }

    public void jump()
    {
        rigidbody.linearVelocityY = base_stats.jump_strength;
    }

    public void dash()
    {
        remaining_dash_frames = 7;
    }

    public void knockback(Vector2 direction)
    {
        remaining_flying_frames = 60;
        rigidbody.linearVelocity = direction;
    }

    public bool jump_action()
    {
        if ((animation_data.flags & FighterFlags.CanJump) == 0) return false;
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


    public bool jab_action(EventInput input)
    {
        if (!input.pressed) return true;

        if (get_action() == FighterAction.JabSide) return false;

        start_action((FighterAction)((int)(FighterAction.JabSide) - input.direction.y));
        return true;
    }

    public bool heavy_action(EventInput input)
    {
        if (!input.pressed) return true;

        start_action((FighterAction)((int)(FighterAction.HeavySide) - input.direction.y));
        return true;
    }

    public bool interact_action(EventInput input)
    {
        return true;
    }

    public bool dash_action()
    {
        dash();
        return true;
    }

    public bool block_action(EventInput input)
    {
        if (!input.pressed)
        {
            if (get_action() == FighterAction.BlockSide || get_action() == FighterAction.BlockUp)
                on_animation_end();
            return true;
        }

        start_action(input.direction.y == 1 ? FighterAction.BlockUp : FighterAction.BlockSide);
        return true;
    }

    public bool ult_action(EventInput input)
    {
        if (!input.pressed) return true;

        start_action(FighterAction.KnockedBackLight);

        knockback(new Vector2(-(float)(int)facing, 2.0f) * 5.0f);
        Debug.Log("knocking back");
        return true;
    }
}
