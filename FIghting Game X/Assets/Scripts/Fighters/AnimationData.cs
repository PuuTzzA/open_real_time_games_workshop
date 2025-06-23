using System;
using UnityEngine;


[Flags]
public enum FighterFlags
{
    None = 0,
    CanTurn = 0x01,
    CanWalk = 0x02,
    CanJump = 0x04,
    Mobile = CanJump | CanTurn | CanWalk,
}

public class AnimationData : MonoBehaviour
{

    public FighterFlags flags;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
