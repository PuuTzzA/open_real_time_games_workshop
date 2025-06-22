public struct BaseStats
{
    public float ground_speed;
    public float horizontal_speed;
    public float vertical_speed;
    public float air_control;
    public float jump_strength;
    public int air_jumps;

    public static readonly BaseStats DEFAULT = new BaseStats
    {
        ground_speed = 7.0f,
        horizontal_speed = 7.0f,
        vertical_speed = 15.0f,
        air_control = 1.0f,
        jump_strength = 12.0f,
        air_jumps = 1,
    };
}