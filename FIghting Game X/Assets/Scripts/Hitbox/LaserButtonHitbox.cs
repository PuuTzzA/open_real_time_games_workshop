using UnityEngine;

public class LaserButtonHitbox : Hitbox
{
    public LaserButton laserButton;
    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !laserButton.isLaserActive && !laserButton.isOnCooldown && !fighter.holdingBomb)
        {
            laserButton.TriggerLaser();
        }
    }
}
