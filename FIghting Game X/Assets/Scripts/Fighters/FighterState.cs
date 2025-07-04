
using UnityEngine;

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
    Stunned,
    Death,
    Crouch
}

public enum Facing
{
    Left = -1,
    Right = 1
}

public class FighterState : MonoBehaviour
{
    public AnimationHandler animation_handler;

    public Transform sprite_transform;
    public BaseStats base_stats;

    public int available_air_jumps;

    public int stun_duration;
    public int knockback_duration;

    public AnimationData animation_data;

    public float air_resistance;

    private bool _grounded;

    private Facing _facing;

    public FighterAction get_action() { return animation_handler.get_action(); }

    public void start_action(FighterAction action)
    {
        animation_handler.play(action);

        //animator.ResetTrigger("trigger");
        //animator.SetInteger("action", (int)action);
        //animator.SetTrigger("trigger");
    }

    public bool is_idle()
    {
        return animation_handler.get_info().idle;
    }

    public bool is_grounded() { return _grounded; }

    public void set_grounded(bool grounded)
    {
        if (grounded)
        {
            available_air_jumps = base_stats.air_jumps;
        }
        _grounded = grounded;
    }


    public Facing get_facing() { return _facing; }
    public int get_facing_int() { return (int)_facing; }
    public float get_facing_float() { return (int)_facing; }
    public Vector2 get_facing_vec() { return new Vector2((int)_facing, 1.0f); }
    public Vector2Int get_facing_ivec() { return new Vector2Int((int)_facing, 1); }

    public void set_facing(Facing dir)
    {
        if (!flags_any_set(FighterFlags.CanTurn)) return;

        _facing = dir;
        var scale = sprite_transform.localScale;
        scale.x = (int)dir;
        sprite_transform.localScale = scale;
    }

    public void set_facing(int dir)
    {
        if ((animation_data.flags & FighterFlags.CanTurn) == 0) return;

        if (dir == 1 || dir == -1)
        {
            _facing = (Facing)dir;
            var scale = sprite_transform.localScale;
            scale.x = dir;
            sprite_transform.localScale = scale;
        }
    }

    public void force_facing(int dir)
    {
        if (dir == 1 || dir == -1)
        {
            _facing = (Facing)dir;
            var scale = sprite_transform.localScale;
            scale.x = dir;
            sprite_transform.localScale = scale;
        }
    }



    public void Awake()
    {
        base_stats = BaseStats.DEFAULT;

        available_air_jumps = base_stats.air_jumps;

        air_resistance = - Physics2D.gravity.y / (base_stats.terminal_speed * base_stats.terminal_speed);

        set_facing(Facing.Right);
        start_action(FighterAction.Idle);
    }


    public bool flags_all_set(FighterFlags flags)
    {
        return (animation_data.flags & flags) == flags;
    }

    public bool flags_any_set(FighterFlags flags)
    {
        return (animation_data.flags & flags) != 0;
    }

    public float get_ground_speed()
    {
        return base_stats.ground_speed;
    }

    public float get_jump_strength()
    {
        return base_stats.jump_strength;
    }

    public float get_air_speed()
    {
        return base_stats.air_speed;
    }

    public float get_terminal_speed()
    {
        return base_stats.terminal_speed;
    }



    public bool can_jump()
    {
        if (!flags_any_set(FighterFlags.CanJump)) return false;
        return is_grounded() || available_air_jumps > 0;
    }



    public FighterSignals read_signals()
    {
        var ret = animation_data.signals;

        animation_data.signals = FighterSignals.None;

        return ret;
    }
}