using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FighterHealth : MonoBehaviour
{

    [Header("UI Settings")]
    public Sprite Icon;

    [Header("Health Settings")]
    public int maxLives = 3;
    public bool qteUsed = false;
    public int maxHealth = 300;

    // Current health and lives
    private int currentLives;
    private int currentHealth;
    private PlayerInput playerInput;
    private PersistentPlayerManager persistentPlayerManager;
    private IngameUI ingameUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ingameUI = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
        currentHealth = maxHealth;
        currentLives = maxLives;
        //ingameUI.changeStocks(playerInput.playerIndex, currentLives);
        persistentPlayerManager = FindFirstObjectByType<PersistentPlayerManager>().GetComponent<PersistentPlayerManager>();
    }

    public void TakeDamage(int dmg, GameObject attacker)
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning("Fighter is already dead. Cannot take more damage.");
            return;
        }

        currentHealth -= dmg;
        ingameUI.setNewHealth(playerInput.playerIndex, currentHealth * 1.0f / maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Start");
            StartCoroutine(HandleDeath(attacker));
        }
    }

    public void TakeArenaDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning("Fighter is already dead. Cannot take more damage.");
            return;
        }

        currentHealth -= damage;
        ingameUI.setNewHealth(playerInput.playerIndex, currentHealth * 1.0f / maxHealth);


        if (currentHealth <= 0)
        {
            HandleArenaDeath();
        }
    }

    private IEnumerator HandleDeath(GameObject killer)
    {
        currentLives--;
        Debug.Log("hallo");
        ingameUI.changeStocks(playerInput.playerIndex, currentLives);
        Debug.Log("thaui");

        Debug.Log($"{this.gameObject.name} has died. Killer: {killer.name}");
        Debug.Log($"Current Lives: {currentLives}, Current Health: {currentHealth}");
        Debug.Log($"QTE Used: {qteUsed}");


        if (currentLives <= 0)
        {
            if (!qteUsed)
            {
                Debug.Log("Fighter died but has another change, win a QTE to stay in the game!");
                yield return DeferQTEStart(killer);
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

    private IEnumerator DeferQTEStart(GameObject killer)
    {
        yield return new WaitForEndOfFrame();
        bool qteFinished = false;
        QTEManager.Instance.StartQTE(this.gameObject, killer, () =>
        {
            qteFinished = true;
        });

        while (!qteFinished)
        {
            yield return null; // Wait until QTE is finished
        }
        Debug.Log("QTE finished.");
    }

    private IEnumerator DelayedHandleDeath(GameObject killer)
    {
        yield return new WaitForSeconds(0.3f); // Delay before handling death
    }

    private IEnumerator DelayedHandleArenaDeath()
    {
        yield return new WaitForSeconds(0.3f); // Delay before handling arena death
        HandleArenaDeath();
    }

    private void HandleArenaDeath()
    {
        currentLives--;
        ingameUI.changeStocks(playerInput.playerIndex, currentLives);



        if (currentLives == 0)
        {
            Debug.Log("Fighter died and has no more lives left. Game Over!");
            Die();
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
        ingameUI.setHealth(playerInput.playerIndex, 1);

    }

    public void GrantExtraLife()
    {
        currentLives = 1;
        ingameUI.changeStocks(playerInput.playerIndex, 4);

        qteUsed = true;
        currentHealth = maxHealth;
    }

    public bool Die()
    {
        // TODO: Handle the death of the fighter, such as disabling controls, playing death animation, etc.
        Debug.Log("Fighter has died. Game Over.");

        List<PlayerInput> playersAlive = persistentPlayerManager.getAlivePlayers();
        Debug.Log($"Players alive: {playersAlive.Count}");

        // Check if there are still more than one fighter alive
        if (playersAlive.Count <= 1)
        {
            Debug.Log("Game is finished, no more fighters left.");

            int winnerIndex = playersAlive.Count == 1 ? playersAlive[0].playerIndex : -1;
            playersAlive[0].SwitchCurrentActionMap("UI");

            var winUI = FindAnyObjectByType<WinGameUI>(FindObjectsInactive.Include);

            if (winUI != null)
            {
                winUI.ShowWinner(winnerIndex);
            }

            return true;
        }

        return false;
    }

    public bool GetFinished(GameObject killer)
    {
        // Function to handle the finishing move to absolutely anihilate the fighter
        // TODO: Play the finishing animation
        Debug.Log($"{killer.name} has finished {this.gameObject.name}!");

        // see if there are still more than one fighter alive
        return Die();
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
