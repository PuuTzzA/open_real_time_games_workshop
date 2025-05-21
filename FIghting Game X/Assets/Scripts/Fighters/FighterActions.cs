
using UnityEngine.InputSystem;

public struct FighterActions
{
    public InputAction direction;
    public InputAction jump;
    public InputAction jab;
    public InputAction heavy;
    public InputAction dash;
    public InputAction block;
    public InputAction ult;

    public void init()
    {
        direction = InputSystem.actions.FindAction("direction");
        jump = InputSystem.actions.FindAction("jump");
        jab = InputSystem.actions.FindAction("jab");
        heavy = InputSystem.actions.FindAction("heavy");
        dash = InputSystem.actions.FindAction("dash");
        block = InputSystem.actions.FindAction("block");
        ult = InputSystem.actions.FindAction("ult");
    }

}