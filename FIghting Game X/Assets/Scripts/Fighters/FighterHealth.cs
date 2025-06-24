using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    
    [Header("Health Settings")]
    public int maxLives = 3;
    public bool qteUsed = false;
    public int maxHealth = 300;
    
    // Current health and lives
    private int currentLives;
    private int currentHealth;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentHealth = maxHealth;
        currentLives = maxLives;
    }

    public void TakeDamage(int dmg, GameObject attacker)
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning("Fighter is already dead. Cannot take more damage.");
            return;
        }
        
        currentHealth -= dmg;
        
        if (currentHealth <= 0)
        {
            HandleDeath(attacker);
        }
    }

    private void HandleDeath(GameObject killer)
    {
        currentLives--;
        
        if (currentLives == 0)
        {
            if (!qteUsed)
            {
                Debug.Log("Fighter died but has another change, win a QTE to stay in the game!");
                QTEManager.Instance.StartQTE(this.gameObject, killer);
            }
            else
            {
                Debug.Log("Fighter died and has no more lives left. Game Over!");
                Die();
            }
        }
        else
        {
            Debug.Log("Fighter died. Remaining lives: " + currentLives);
            Respawn();
        }
    }
    
    private void Respawn()
    {
        currentHealth = maxHealth;
        Debug.Log("Fighter respawned with full health.");
        // TODO 
        
    }

    public void GrantExtraLife()
    {
        currentLives = 1;
        qteUsed = true;
        currentHealth = maxHealth;
    }

    public void Die()
    {
        // TODO: Handle the death of the fighter, such as disabling controls, playing death animation, etc.
        Debug.Log("Fighter has died. Game Over.");
        this.gameObject.SetActive(false);
    }

    public void GetFinished(GameObject killer)
    {
        // Function to handle the finishing move to absolutely anihilate the fighter
        // TODO: Play the finishing animation
    }
    
    
}
