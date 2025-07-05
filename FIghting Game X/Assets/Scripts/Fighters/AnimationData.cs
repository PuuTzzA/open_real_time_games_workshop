using System;
using UnityEngine;


[Flags]
public enum FighterFlags
{
    None = 0,
    CanTurn = 0x001,
    CanMove = 0x002,
    CanJump = 0x004,
    Mobile = CanJump | CanTurn | CanMove,

    Interruptable = 0x008,

    BlockSide = 0x010,
    BlockUp = 0x020,

    FreezeX = 0x040,
    FreezeY = 0x080,

    CustomMovement = 0x100,
    Phasing = 0x200,


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
