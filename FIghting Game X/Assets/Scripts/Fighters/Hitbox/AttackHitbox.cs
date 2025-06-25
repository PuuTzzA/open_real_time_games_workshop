using UnityEngine;

public class AttackHitbox : Hitbox
{
    public float damage;
    public Vector2 knockback;
    public Vector2Int direction;
    public BaseFighter source_fighter;

    public override void hit(BaseFighter fighter)
    {
        if(fighter.is_blocking(direction * source_fighter.state.get_facing_ivec())) { return; }
        int forced_facing = -direction.x * source_fighter.state.get_facing_int();
        fighter.state.force_facing(forced_facing);
        fighter.knockback(knockback * source_fighter.state.get_facing_vec());
    }
}