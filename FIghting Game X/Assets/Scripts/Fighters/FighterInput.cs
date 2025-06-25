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
        public EventType type;
        public bool pressed;
        public Vector2Int direction;
    }

    private Queue<Event> event_queue;

    public EventBuffer event_buffer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    FighterInput()
    {
        direction = Vector2Int.zero;

        jump = jab = heavy = interact = dash = block = ult = false;

        event_queue = new Queue<Event>();

        event_buffer = new EventBuffer();
    }

    public void dispatch_events()
    {
        foreach (var e in event_queue)
        {
            event_buffer.push(new FighterEvent(new EventData { type = e.type, pressed = e.pressed, direction = e.type == EventType.Direction ? e.direction : direction }));
        }
        event_queue.Clear();
    }

    public void direction_action(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        Vector2Int new_dir;

        if (dir.magnitude < 0.2)
        {
            new_dir = Vector2Int.zero;
        }
        else
        {
            var dir_normal = dir.normalized;

            dir *= 1.0f / Mathf.Max(Mathf.Abs(dir_normal.x), Mathf.Abs(dir_normal.y));

            new_dir = new Vector2Int(
                dir.x < -0.7 ? -1 : dir.x > 0.7 ? 1 : 0,
                dir.y < -0.7 ? -1 : dir.y > 0.7 ? 1 : 0
            );

        }


        if (!new_dir.Equals(direction))
        {
            event_queue.Enqueue(new Event { type = EventType.Direction, direction = new_dir });
        }
        direction = new_dir;
    }

    public void jump_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            jump = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Jump, pressed = context.started });
        }
    }


    public void jab_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            jab = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Jab, pressed = context.started });
        }
    }

    public void heavy_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            heavy = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Heavy, pressed = context.started });
        }
    }

    public void interact_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            interact = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Interact, pressed = context.started });
        }
    }

    public void dash_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            dash = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Dash, pressed = context.started });
        }
    }

    public void block_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            block = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Block, pressed = context.started });
        }
    }

    public void ult_action(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            ult = context.ReadValueAsButton();
            event_queue.Enqueue(new Event { type = EventType.Ult, pressed = context.started });
        }
    }
}
