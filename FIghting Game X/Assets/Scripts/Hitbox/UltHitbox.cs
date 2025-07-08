using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UltHitbox : Hitbox
{
    public BaseFighter source_fighter;

    public Dictionary<BaseFighter, int> hit_fighters;
    public Dictionary<BaseFighter, int> fighter_cooldowns;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (type != HitType.Start) { return; }

        if(!hit_fighters.ContainsKey(fighter))
        {
            hit_fighters.Add(fighter, 0);
        }

        if (!fighter_cooldowns.ContainsKey(fighter))
        {
            fighter_cooldowns.Add(fighter, 0);
        }

        if (fighter_cooldowns[fighter] <= 0)
        {
            hit_fighters[fighter]++;
            fighter_cooldowns[fighter] = 18;
        }
        fighter.stun(10);

        fighter.take_damage(10, source_fighter.gameObject);
    }

    public void knockback_fighters()
    {
        foreach(var (fighter, hits) in hit_fighters)
        {
            var sign = Math.Sign(fighter.transform.position.x - source_fighter.transform.position.x);
            var knockback = new Vector2(3.0f * (sign == 0 ? 1 : sign), 5.0f) * (1 + hits * 0.5f);
            Debug.Log(knockback + " hits: " + hits);
            fighter.knockback_heavy(knockback, 10 + hits * 2);
            fighter.take_damage(10 + hits * 5, source_fighter.gameObject);
        }
    }

    public void reduce_fighter_cooldowns()
    {

        var keys = fighter_cooldowns.Keys.ToArray();
        foreach(var key in keys)
        {
            fighter_cooldowns[key]--;
        }
    }

    public void init()
    {
        hit_fighters = new Dictionary<BaseFighter, int>();
        fighter_cooldowns = new Dictionary<BaseFighter, int>();
    }
}
