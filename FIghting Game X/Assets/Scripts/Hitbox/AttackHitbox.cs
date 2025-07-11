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
    public int ult_points;

    public HashSet<BaseFighter> hit_fighters = new HashSet<BaseFighter>();

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (type != HitType.Start || hit_fighters.Contains(fighter)) { return; }
        if (fighter.is_blocking(direction * source_fighter.state.get_facing_ivec())) { return; }

        if (jab)
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
        int forced_facing = -source_fighter.state.get_facing_int();
        fighter.state.force_facing(forced_facing);

        fighter.knockback_light(knockback * source_fighter.state.get_facing_vec());
        source_fighter.state.add_ult_points(ult_points);

        if (direction.Equals(Vector2Int.down))
        {
            source_fighter.rigidbody.linearVelocityY = 7.0f;
        }
        fighter.take_damage(damage, this.source_fighter.gameObject);
    }

    private void heavy_hit(BaseFighter fighter)
    {
        
        fighter.knockback_heavy(knockback * source_fighter.state.get_facing_vec(), duration);

        source_fighter.state.add_ult_points(ult_points);
        fighter.take_damage(damage, this.source_fighter.gameObject);
    }
}