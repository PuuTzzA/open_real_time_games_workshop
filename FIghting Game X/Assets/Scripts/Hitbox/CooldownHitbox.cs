
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CooldownHitbox : Hitbox
{
    public Dictionary<int, int> fighter_cooldowns = new Dictionary<int, int>();
    public int cooldown = 20;

    public void FixedUpdate()
    {
        var keys = fighter_cooldowns.Keys.ToArray();
        foreach (var key in keys)
        {
            fighter_cooldowns[key] = Mathf.Max(fighter_cooldowns[key] - 1, 0);
        }
    }

    public bool can_be_hit(int fighter_id)
    {
        if (!fighter_cooldowns.ContainsKey(fighter_id))
        {
            fighter_cooldowns.Add(fighter_id, 0);
        }

        return fighter_cooldowns[fighter_id] <= 0;
    }

    public void put_on_cooldown(int fighter_id)
    {
        Debug.Log("cooldown: " + cooldown);
        fighter_cooldowns[fighter_id] = cooldown;
    }

    public virtual void init()
    {
        fighter_cooldowns = new Dictionary<int, int>();
    }
}