using UnityEngine;

public class BombHitbox : Hitbox
{
    public Bomb bomb;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (bomb.HasHolder()) return;
        
        if (fighter.fighter_input.interact && !bomb.HasHolder() && !fighter.holdingBomb && !bomb.thrown)
        {
            bomb.Pickup(fighter);
        }
    }
}