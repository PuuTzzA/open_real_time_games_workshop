using UnityEngine;

public class LaserButtonHitbox : Hitbox
{
    public LaserButton laserButton;

    private bool lastFrame = false;
    private bool thisFrame = false;

    private BaseFighter _fighter;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !laserButton.isLaserActive && !laserButton.isOnCooldown && !fighter.holdingBomb)
        {
            laserButton.TriggerLaser();
        }
        else if (!laserButton.isLaserActive && !laserButton.isOnCooldown && !fighter.holdingBomb)
        {
            _fighter = fighter;
            thisFrame = true;
        }
    }
    
    private void FixedUpdate()
    {
        if (thisFrame && !lastFrame)
        {
            laserButton.TriggerActivatable(_fighter);
        }
        if (!thisFrame && lastFrame)
        {
            laserButton.TriggerDisableActivatable();
        }

        lastFrame = thisFrame;
        thisFrame = false;
    }
}
