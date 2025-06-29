using System;
using UnityEngine;


[Flags]
public enum FighterFlags
{
    None = 0,
    CanTurn = 0x01,
    CanMove = 0x02,
    CanJump = 0x04,
    Mobile = CanJump | CanTurn | CanMove,

    Interruptable = 0x08,

    BlockSide = 0x10,
    BlockUp = 0x20,

    FreezeX = 0x40,
    FreezeY = 0x80,


    Idle = CanJump | CanTurn | CanMove | Interruptable,
}

[Flags]
public enum FighterSignals
{
    None = 0,
    Finished = 0x01,
    ShouldJump = 0x02,
}

public class AnimationData : MonoBehaviour
{

    public FighterFlags flags;
    public FighterSignals signals;

    public Animator animator;

    public void signal_finished()
    {
        signals |= FighterSignals.Finished;
    }

    public void signal_jump()
    {
        signals |= FighterSignals.ShouldJump;
    }

    public void freeze_frame()
    {
        animator.speed = 0;
    }
}
