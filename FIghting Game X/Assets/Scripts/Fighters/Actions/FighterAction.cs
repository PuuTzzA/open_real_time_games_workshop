using System;
using System.Runtime.CompilerServices;
using UnityEngine;


public class FighterAction
{
    public double elapsed = 0.0;
    public double factor = 1.0;
    public bool loop = true;

    public ActionFrame[] frames;
    private BaseFighter fighter;

    public FighterAction(BaseFighter fighter)
    {
        this.fighter = fighter;
    }

    public ActionFrame current_frame()
    {
        int frame_idx = (int)(elapsed * factor);

        if(loop)
            return frames[frame_idx % frames.Length];
        else
            return frames[Math.Clamp(frame_idx, 0, frames.Length - 1)];

    }

    public void next()
    {
        elapsed += 1.0f;
    }
}

[Flags]
public enum StateFlags: byte
{
    None =          0b_0000_0000,
    Interruptable = 0b_0000_0001,
    CanTurn =       0b_0000_0010,
    CanMove =       0b_0000_0100,
    CanJump =       0b_0000_1000,
}

public class ActionFrame
{
    public StateFlags flags = StateFlags.None;
    public Sprite sprite;
};