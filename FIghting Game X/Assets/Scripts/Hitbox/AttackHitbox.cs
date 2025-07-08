using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : Hitbox
{
    public int damage;
    public Vector2 knockback;
    public Vector2Int direction;
    public BaseFighter source_fighter;
    public bool jab = true;
    public int duration = 10;

    public HashSet<BaseFighter> hit_fighters = new HashSet<BaseFighter>();

    public override void hit(BaseFighter fighter, HitType type)
    {
        Debug.Log("hit " + hit_fighters.Contains(fighter));
        if (type != HitType.Start || hit_fighters.Contains(fighter)) { return; }

        if(jab)
        {
            jab_hit(fighter);
        } else
        {
            heavy_hit(fighter);
        }

        hit_fighters.Add(fighter);
    }

    private void jab_hit(BaseFighter fighter)
    {
        if (fighter.is_blocking(direction * source_fighter.state.get_facing_ivec())) { return; }

        int forced_facing = -source_fighter.state.get_facing_int();
        fighter.state.force_facing(forced_facing);

        fighter.knockback_light(knockback * source_fighter.state.get_facing_vec());
        fighter.take_damage(damage, this.source_fighter.gameObject);

        if (direction.Equals(Vector2Int.down))
        {
            source_fighter.rigidbody.linearVelocityY = 7.0f;
        }
    }

    private void heavy_hit(BaseFighter fighter)
    {
        
        fighter.knockback_heavy(knockback * source_fighter.state.get_facing_vec(), duration);
        fighter.take_damage(damage, this.source_fighter.gameObject);
    }
}