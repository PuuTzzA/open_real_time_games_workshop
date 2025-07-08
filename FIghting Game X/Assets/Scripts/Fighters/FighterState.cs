
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    Crouch,
    UltHammer
}

public enum Facing
{
    Left = -1,
    Right = 1
}

public class UltData
{
    public float angle;
    public int index;

    public UltData(float angle, int index)
    {
        this.angle = angle;
        this.index = index;
    }
}

public class FighterState : MonoBehaviour
{
    public AnimationHandler animation_handler;

    public Transform sprite_transform;
    public BaseStats base_stats;

    public int available_air_jumps;

    public int stun_duration;

    public int knockback_duration;
    public static readonly float[] knockback_rotation_factors = {0.0f, 0.3f, 0.7f, 1.0f, 0.9f, 0.7f, 0.3f, 0.1f, 0.0f };

    public AnimationHandler hammer_animation_handler;
    public Transform hammer_base_transform;
    public UltHitbox ult_hitbox;
    public UltData[] ult_data;
    public int ult_index;

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

        init_ult_data();

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

    public void init_ult_data()
    {
        List<UltData> data = new List<UltData>();

        for (int i = 0; i < 25; i++)
        {
            data.Add(new UltData(0, i));
        }

        for (int i = 0; i < 25; i++)
        {
            data.Add(new UltData(5.0f * (i * i) / 24.0f, data.Count));
        }

        for (int i = 0; i < 127; i++)
        {
            data.Add(new UltData((130 + i * 10) % 360, 51));
        }

        for (int i = 0; i < 11; i++)
        {
            data.Add(new UltData(360.0f - ((10 - i) * (10 - i) * 0.5f), 52 + i));
        }

        ult_data = data.ToArray();
    }
}