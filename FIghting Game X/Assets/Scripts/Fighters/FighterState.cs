public struct FighterState
{
    public CurrentAction action;

    public static FighterState create()
    {
        return new FighterState { action = CurrentAction.NoAction };
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