using System;
using System.Collections.Generic;
using UnityEngine;

public struct EventInput
{
    public bool pressed;
    public Vector2Int direction;
}

public class FighterEvent
{
    public const int DEFAULT_TTL = 8;

    public FighterButton button;
    public EventInput input;
    public int ttl;

    public FighterEvent(FighterButton button, EventInput input, int ttl = DEFAULT_TTL)
    {
        this.button = button;
        this.input = input;
        this.ttl = ttl;
    }
}


public class EventBuffer
{
    private List<FighterEvent> buffered_actions;

    private Func<EventInput, bool>[] callbacks;

    public EventBuffer()
    {
        buffered_actions = new List<FighterEvent>();

        callbacks = new Func<EventInput, bool>[Enum.GetValues(typeof(FighterButton)).Length];
    }

    public void register(FighterButton button, Func<EventInput, bool> callback)
    {
        callbacks[(int)button] = callback;
    }

    public void register(FighterButton button, Func<bool> callback)
    {
        callbacks[(int)button] = input =>  input.pressed ? callback() : true;
    }


    public void push(FighterEvent action)
    {
        buffered_actions.Add(action);
    }


    public void process()
    {
        List<FighterEvent> new_buffer = new List<FighterEvent> ();

        foreach (var action in buffered_actions)
        {
            if(action.ttl-- > 0 && !callbacks[(int)action.button](action.input))
                new_buffer.Add(action);
        }

        buffered_actions = new_buffer;
    }
}