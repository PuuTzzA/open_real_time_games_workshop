using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UltHitbox : CooldownHitbox
{
    public BaseFighter source_fighter;

    public Dictionary<BaseFighter, int> hit_fighters;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (type != HitType.Start) { return; }

        foreach (var (key, val) in fighter_cooldowns)
        {
            Debug.Log(val);
        }

        if(!hit_fighters.ContainsKey(fighter))
        {
            hit_fighters.Add(fighter, 0);
        }

        if (can_be_hit(fighter.id))
        {
            hit_fighters[fighter]++;
            put_on_cooldown(fighter.id);
            fighter.stun(10);
            fighter.take_damage(10, source_fighter.gameObject);
        }

        foreach (var (key, val) in fighter_cooldowns)
        {
            Debug.Log(val);
        }

    }

    public void knockback_fighters()
    {
        foreach(var (fighter, hits) in hit_fighters)
        {
            Debug.Log("hits: " +  hits);
            var sign = Math.Sign(fighter.transform.position.x - source_fighter.transform.position.x);
            //var knockback = new Vector2(3.0f * (sign == 0 ? 1 : sign), 5.0f) * (2 + hits * 0.5f);
            var knockback = (fighter.transform.position - source_fighter.transform.position).normalized * 5.0f * (2 + hits * 0.5f);
            fighter.knockback_heavy(knockback, 14 + hits * 2);
            fighter.take_damage(10 + hits * 5, source_fighter.gameObject);
        }

        hit_fighters.Clear();
    }

    public override void init()
    {
        base.init();
        hit_fighters = new Dictionary<BaseFighter, int>();
    }
}
