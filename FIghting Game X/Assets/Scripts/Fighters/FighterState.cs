
using UnityEngine;

public class FighterState : MonoBehaviour
{
    public Animator animator;
    public Transform sprite_transform;
    public BaseStats base_stats = BaseStats.DEFAULT;

    public int available_air_jumps;
    public int remaining_dash_frames;
    public float dash_speed;
    public int remaining_flying_frames;

    public AnimationData animation_data;

    public bool passive = true;

    private FighterAction _current_action;

    private bool _grounded;

    private Facing _facing;

    public FighterAction get_action() { return _current_action; }

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

    public void set_facing(Facing dir)
    {
        if ((animation_data.flags & FighterFlags.CanTurn) == 0) return;

        _facing = dir;
        var scale = sprite_transform.localScale;
        scale.x = (int)dir;
        sprite_transform.localScale = scale;
    }



    public void Start()
    {
        available_air_jumps = base_stats.air_jumps;
        remaining_dash_frames = 0;

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

    public Vector2 get_air_speed()
    {
        return base_stats.air_speed;
    }



    public bool can_jump()
    {
        if (!flags_any_set(FighterFlags.CanJump)) return false;
        return is_grounded() || available_air_jumps > 0;
    }


}