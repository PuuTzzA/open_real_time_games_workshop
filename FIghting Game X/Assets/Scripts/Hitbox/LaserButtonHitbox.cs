using UnityEngine;

public class LaserButtonHitbox : Hitbox
{
    public LaserButton laserButton;

    private bool lastFrame = false;
    private bool thisFrame = false;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !laserButton.isLaserActive && !laserButton.isOnCooldown && !fighter.holdingBomb)
        {
            laserButton.TriggerLaser();
        }
        else if (!laserButton.isLaserActive && !laserButton.isOnCooldown && !fighter.holdingBomb)
        {
            thisFrame = true;
        }
    }
    
    private void FixedUpdate()
    {
        if (thisFrame && !lastFrame)
        {
            laserButton.TriggerActivatable();
        }
        if (!thisFrame && lastFrame)
        {
            laserButton.TriggerDisableActivatable();
        }

        lastFrame = thisFrame;
        thisFrame = false;
    }
}
