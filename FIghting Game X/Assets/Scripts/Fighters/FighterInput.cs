using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum FighterButton
{
    Jump, Jab, Heavy, Interact, Dash, Block, Ult
}

public class FighterInput : MonoBehaviour
{

    public Vector2Int direction { get; private set; }

    public bool jump { get; private set; }
    public bool jab { get; private set; }
    public bool heavy { get; private set; }
    public bool interact { get; private set; }
    public bool dash { get; private set; }
    public bool block { get; private set; }
    public bool ult { get; private set; }

    private struct Event
    {
        public FighterButton button;
        public bool pressed;
    } 

    private Queue<Event> event_queue;

    private Action<bool>[] callbacks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    FighterInput()
    {
        direction = Vector2Int.zero;

        jump = jab = heavy = interact = dash = block = ult = false;

        event_queue = new Queue<Event>();
        callbacks = new Action<bool>[Enum.GetValues(typeof(FighterButton)).Length];
    }

    // Update is called once per frame
    public void dispatch_events()
    {
        foreach (var e in event_queue)
        {
            callbacks[(int)e.button](e.pressed);
        }
        event_queue.Clear();
    }

    public void set_callback(FighterButton button, Action<bool> callback)
    {
        callbacks[(int)button] = callback;
    }

    public void set_callback(FighterButton button, System.Action callback)
    {
        callbacks[(int)button] = pressed => { if (pressed) callback(); };
    }

    public void direction_action(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();

        if(dir.magnitude < 0.2)
        {
            direction = Vector2Int.zero;
            return;
        }

        var dir_normal = dir.normalized;

        dir *= 1.0f / Mathf.Max(Mathf.Abs(dir_normal.x), Mathf.Abs(dir_normal.y));

        direction = new Vector2Int(
            dir.x < -0.7 ? -1 : dir.x > 0.7 ? 1 : 0,
            dir.y < -0.7 ? -1 : dir.y > 0.7 ? 1 : 0
        );

    }

    public void jump_action(InputAction.CallbackContext context)
    {
        jump = context.ReadValueAsButton();

        if ( !context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started});
        }
    }


    public void jab_action(InputAction.CallbackContext context)
    {
        jab = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }

    public void heavy_action(InputAction.CallbackContext context)
    {
        heavy = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }

    public void interact_action(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }

    public void dash_action(InputAction.CallbackContext context)
    {
        dash = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }

    public void block_action(InputAction.CallbackContext context)
    {
        block = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }

    public void ult_action(InputAction.CallbackContext context)
    {
        ult = context.ReadValueAsButton();

        if (!context.performed)
        {
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started });
        }
    }
}
