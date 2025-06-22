using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    FighterInput()
    {
        direction = Vector2Int.zero;

        jump = jab = heavy = interact = dash = block = ult = false;

        event_queue = new Queue<Event>();
    }

    public void dispatch_events(EventBuffer action_buffer)
    {
        foreach (var e in event_queue)
        {
            action_buffer.push(new FighterEvent(e.button, new EventInput { pressed = e.pressed, direction = direction }));
        }
        event_queue.Clear();
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
        if ( !context.performed)
        {
            jump = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Jump, pressed = context.started});
        }
    }


    public void jab_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            jab = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Jab, pressed = context.started });
        }
    }

    public void heavy_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            heavy = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Heavy, pressed = context.started });
        }
    }

    public void interact_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            interact = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Interact, pressed = context.started });
        }
    }

    public void dash_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            dash = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Dash, pressed = context.started });
        }
    }

    public void block_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            block = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Block, pressed = context.started });
        }
    }

    public void ult_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            ult = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { button = FighterButton.Ult, pressed = context.started });
        }
    }
}
