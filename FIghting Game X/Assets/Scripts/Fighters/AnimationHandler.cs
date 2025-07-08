using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public enum AnimationEndAction
{
    Loop,
    Wait,
    Signal
}

public class AnimationInfo
{
    public string state_name;
    public AnimationEndAction end_action;
    public bool idle;

    public AnimationInfo(string state_name, AnimationEndAction end_action, bool idle = false)
    {
        this.state_name = state_name;
        this.end_action = end_action;
        this.idle = idle;
    }
}

public class AnimationHandler : MonoBehaviour
{
    public Animator animator;
    private static readonly AnimationInfo[] state_infos = {
        new ("idle", AnimationEndAction.Loop, true),
        new ("running", AnimationEndAction.Loop, true),
        new ("jump", AnimationEndAction.Signal),
        new ("jab_up", AnimationEndAction.Signal),
        new ("jab_side", AnimationEndAction.Signal),
        new ("jab_down", AnimationEndAction.Signal),
        new ("falling", AnimationEndAction.Loop, true),
        new ("ultimate", AnimationEndAction.Signal),
        new ("emote", AnimationEndAction.Signal),
        new ("block_up", AnimationEndAction.Wait),
        new ("block_side", AnimationEndAction.Wait),
        new ("heavy_up", AnimationEndAction.Signal),
        new ("heavy_side", AnimationEndAction.Signal),
        new ("heavy_down", AnimationEndAction.Signal),
        new ("dash", AnimationEndAction.Signal),
        new ("knocked_back_light", AnimationEndAction.Signal),
        new ("knocked_back_heavy", AnimationEndAction.Signal),
        new ("stunned", AnimationEndAction.Signal),
        new ("die", AnimationEndAction.Signal),
        new ("crouch", AnimationEndAction.Wait),
        new ("ult_hammer", AnimationEndAction.Signal),
    };

    public FighterAction default_fighter_action;

    private int _current_frame_count;
    private int _frame_index;
    private FighterAction _action;
    private AnimationInfo _info;

    private bool _frozen = false;
    private bool _finished;

    private Action<int>[] _callbacks;

    public FighterAction get_action() { return _action; }
    public AnimationInfo get_info() { return _info; }

    public bool is_frozen() { return _frozen; }
    public void set_frozen(bool frozen) {  _frozen = frozen; }

    public bool is_finished() { return _finished; }
    public int get_index() { return _frame_index; }

    private void Awake()
    {
        animator.speed = 0.0f;

        play(default_fighter_action);
    }

    public void play(FighterAction action)
    {
        _info = state_infos[(int)action];
        _action = action;
        animator.Play(_info.state_name, 0, 0.0f);
        animator.Update(0.0f);

        var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        _current_frame_count = Mathf.RoundToInt(clip.frameRate * clip.length);
        _frame_index = 0;
        _finished = false;
        _frozen = false;
    }

    public void show()
    {
        float normalized_time = _frame_index / (float)_current_frame_count;
        animator.Play(_info.state_name, 0, normalized_time);
        animator.Update(0.0f);
    }

    public void step()
    {
        if (!_frozen)
            _frame_index++;

        switch (_info.end_action)
        {
            case AnimationEndAction.Loop:
                _frame_index %= _current_frame_count;
                break;
            case AnimationEndAction.Wait:
                if (_frame_index >= _current_frame_count)
                    _frame_index = _current_frame_count - 1;
                break;
            case AnimationEndAction.Signal:
                if (_frame_index >= _current_frame_count)
                {
                    _frame_index = _current_frame_count - 1;
                    _finished = true;
                }
                break;
        }
    }

    public void set_index(int index)
    {
        _frame_index = Math.Clamp(index, 0, _current_frame_count - 1);
    }
}
