using NUnit.Framework;
using System;
using System.Collections.Generic;

public class DelayedAction
{
    public Action action;
    public int delay;

    public DelayedAction(Action action, int delay)
    {
        this.action = action;
        this.delay = delay;
    }
}

public class DelayedActions
{
    private List<DelayedAction> delayed_actions;

    public DelayedActions()
    {
        delayed_actions = new List<DelayedAction>();
    }

    public void push(DelayedAction action)
    {
        delayed_actions.Add(action);
    }

    public void tick()
    {
        var new_actions = new List<DelayedAction>();
        foreach (DelayedAction action in delayed_actions)
        {
            if(action.delay-- == 0)
            {
                action.action();
            } else
            {
                new_actions.Add(action);
            }
        }

        delayed_actions = new_actions;
    }
}