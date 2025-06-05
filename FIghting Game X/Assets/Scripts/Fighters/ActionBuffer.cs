using System;
using System.Collections.Generic;
using UnityEngine;

public struct ActionInput
{
    public bool pressed;
    public Vector2Int direction;
}

public class FighterAction
{
    public const int DEFAULT_TTL = 10;

    public FighterButton button;
    public ActionInput input;
    public int ttl;

    public FighterAction(FighterButton button, ActionInput input, int ttl = DEFAULT_TTL)
    {
        this.button = button;
        this.input = input;
        this.ttl = ttl;
    }
}


public class ActionBuffer
{
    private List<FighterAction> buffered_actions;

    private Func<ActionInput, bool>[] callbacks;

    public ActionBuffer()
    {
        buffered_actions = new List<FighterAction>();

        callbacks = new Func<ActionInput, bool>[Enum.GetValues(typeof(FighterButton)).Length];
    }

    public void register(FighterButton button, Func<ActionInput, bool> callback)
    {
        callbacks[(int)button] = callback;
    }

    public void register(FighterButton button, Func<bool> callback)
    {
        callbacks[(int)button] = input =>  input.pressed ? callback() : true;
    }


    public void push(FighterAction action)
    {
        buffered_actions.Add(action);
    }


    public void process()
    {
        List<FighterAction> new_buffer = new List<FighterAction> ();

        foreach (var action in buffered_actions)
        {
            if(action.ttl-- > 0 && !callbacks[(int)action.button](action.input))
                new_buffer.Add(action);
        }

        buffered_actions = new_buffer;
    }
}