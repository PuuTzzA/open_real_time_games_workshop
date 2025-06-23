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
}

public class AnimationData : MonoBehaviour
{

    public FighterFlags flags;
    public bool finished = false;

    public void signal_finished()
    {
        finished = true;
    }
}
