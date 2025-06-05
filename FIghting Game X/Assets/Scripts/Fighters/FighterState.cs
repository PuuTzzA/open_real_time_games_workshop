public struct FighterState
{
    public bool grounded;
    public Facing facing;
    public CurrentAction action;

    public static FighterState create()
    {
        return new FighterState { grounded = false, facing = Facing.Right, action = CurrentAction.NoAction };
    }
}

public enum Facing
{
    Left = -1,
    Right = 1,
}

public enum CurrentAction
{
    NoAction,
    Jab,
    Heavy,
    Block,
    Dash,
    Ult
}