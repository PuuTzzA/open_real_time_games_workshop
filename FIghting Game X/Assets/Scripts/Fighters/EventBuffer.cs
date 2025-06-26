using System;
using System.Collections.Generic;
using UnityEngine;

public struct EventData
{
    public EventType type;
    public bool pressed;
    public Vector2Int direction;
}

public class FighterEvent
{
    public const int DEFAULT_TTL = 12;

    public EventData data;
    public int ttl;

    public FighterEvent(EventData data, int ttl = DEFAULT_TTL)
    {
        this.data = data;
        this.ttl = ttl;
    }
}


public class EventBuffer
{
    private List<FighterEvent> buffered_actions;

    private List<Func<EventData, bool>>[] callbacks;

    public EventBuffer()
    {
        buffered_actions = new List<FighterEvent>();

        callbacks = new List<Func<EventData, bool>>[Enum.GetValues(typeof(EventType)).Length];
        for(int i = 0; i < callbacks.Length; i++)
        {
            callbacks[i] = new List<Func<EventData, bool>>();
        }
    }

    public void register(EventType button, Func<EventData, bool> callback)
    {
        callbacks[(int)button].Add(callback);
    }

    public void register(EventType button, Func<bool> callback)
    {
        callbacks[(int)button].Add(input =>  input.pressed ? callback() : true);
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
            foreach(var callback in callbacks[(int)action.data.type])
            if(action.ttl-- > 0 && !callback(action.data))
                new_buffer.Add(action);
        }

        buffered_actions = new_buffer;
    }
}