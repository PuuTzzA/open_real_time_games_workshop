using UnityEngine;

public struct BaseStats
{
    public float ground_speed;
    public float dash_factor;
    public int dash_duration;
    public float air_speed;
    public float terminal_speed;
    public float air_control;
    public float jump_strength;
    public int air_jumps;

    public static readonly BaseStats DEFAULT = new BaseStats
    {
        ground_speed = 7.0f,
        dash_factor = 3.0f,
        dash_duration = 7,
        air_speed = 7.0f,
        terminal_speed = -10.0f,
        air_control = 1.0f,
        jump_strength = 12.0f,
        air_jumps = 1,
    };
}