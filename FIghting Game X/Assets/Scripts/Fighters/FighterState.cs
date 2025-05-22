public struct FighterState
{
    public bool grounded;
    public Facing facing;
    public FighterAction action;
}

public enum Facing
{
    LEFT = -1,
    RIGHT = 1,
}

public enum FighterAction
{
    NO_ACTION,
    JAB,
    HEAVY,
    BLOCK,
    DASH,
    ULT
}