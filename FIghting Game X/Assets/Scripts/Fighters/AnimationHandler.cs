using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

enum AnimationEndAction
{
    Loop,
    Wait,
    GoIdle
}

public class AnimationHandler : MonoBehaviour
{
    public Animator animator;
    private static readonly (string, AnimationEndAction)[] state_infos = {
        ("idle", AnimationEndAction.Loop),
        ("running", AnimationEndAction.Loop),
        ("jump", AnimationEndAction.GoIdle),
        ("jab_up", AnimationEndAction.GoIdle),
        ("jab_side", AnimationEndAction.GoIdle),
        ("jab_down", AnimationEndAction.GoIdle),
        ("falling", AnimationEndAction.Loop),
        ("ult", AnimationEndAction.GoIdle),
        ("emote", AnimationEndAction.GoIdle),
        ("block_up", AnimationEndAction.Wait),
        ("block_side", AnimationEndAction.Wait),
        ("heavy_up", AnimationEndAction.GoIdle),
        ("heavy_side", AnimationEndAction.GoIdle),
        ("heavy_down", AnimationEndAction.GoIdle),
        ("dash", AnimationEndAction.GoIdle),
        ("knocked_back_light", AnimationEndAction.GoIdle),
        ("knocked_back_heavy", AnimationEndAction.GoIdle),
        ("stunned", AnimationEndAction.GoIdle),
        ("die", AnimationEndAction.GoIdle)
    };

    private int _current_frame_count;
    private int _frame_index;
    private FighterAction _action;
    private AnimationEndAction _end_action;

    private void Awake()
    {
        animator.speed = 0.0f;
    }

    public void play(FighterAction action)
    {
        var (state_name, end_action) = state_infos[(int)action];
        _action = action;
        animator.Play(state_name, 0, 0.0f);
        animator.Update(0.0f);

        var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        _current_frame_count = Mathf.RoundToInt(clip.frameRate * clip.length);
        _end_action = end_action;
        _frame_index = 0;
    }

    public FighterAction get_action() { return _action; }

    public void tick()
    {
        switch (_end_action)
        {
            case AnimationEndAction.Loop:
                _frame_index %= _current_frame_count;
                break;
            case AnimationEndAction.Wait:
                if (_frame_index < 0)
                    _frame_index = 0;
                if (_frame_index >= _current_frame_count)
                    _frame_index = _current_frame_count - 1;
                break;
            case AnimationEndAction.GoIdle:
                if (_frame_index >= _current_frame_count)
                {
                    play(FighterAction.Idle);
                }
                break;
        }

        float normalized_time = _frame_index / (float)_current_frame_count;
        animator.Play(state_infos[(int)_action].Item1, 0, normalized_time);
        animator.Update(0.0f);
        _frame_index++;
    }
}
