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

    UseGravity = 0x08,

    BlockSide = 0x10,
    BlockUp = 0x20,

    Interruptable = 0x40,

    Idle = CanJump | CanTurn | CanMove | UseGravity | Interruptable,
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

    public void signal_finished()
    {
        signals |= FighterSignals.Finished;
    }

    public void signal_jump()
    {
        signals |= FighterSignals.ShouldJump;
    }
}
