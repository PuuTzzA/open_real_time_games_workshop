using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FighterHealth : MonoBehaviour
{


    [Header("UI Settings")]
    public Sprite Icon;
    public Sprite IconWithoutBackground;

    [Header("Health Settings")]
    public int maxLives = 3;
    public bool qteUsed = false;
    public int maxHealth = 10;

    // Current health and lives
    private int currentLives;
    private int currentHealth;
    private PlayerInput playerInput;
    private PersistentPlayerManager persistentPlayerManager;
    private IngameUI ingameUI;
    private FighterState fighterState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        fighterState = GetComponent<FighterState>();
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
            return;
        }

        currentHealth -= dmg;
        // Debug.Log("dmg: " + dmg + " health: " + currentHealth);
        ingameUI.setNewHealth(playerInput.playerIndex, currentHealth * 1.0f / maxHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeath(attacker));
        }
    }

    public void TakeArenaDamage(int damage)
    {
        if (currentHealth <= 0)
        {
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
        ingameUI.changeStocks(playerInput.playerIndex, currentLives);
        fighterState.start_action(FighterAction.Death);


        if (currentLives <= 0)
        {
            if (!qteUsed)
            {
                yield return DeferQTEStart(killer);
            }
            else
            {
                Die();
            }
        }
        else
        {
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
    }

    private void HandleArenaDeath()
    {
        currentLives--;
        ingameUI.changeStocks(playerInput.playerIndex, currentLives);
        fighterState.start_action(FighterAction.Death);

        // Hide the fighter temporarily
        toggleFighter(false);
        

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            Respawn();
        }
    }

    // Disappear the fighter for about 1 seconds and then respawn
    private void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {


        yield return new WaitForSeconds(1.5f); // Delay before respawning

        // Reactivate the fighter
        toggleFighter(true);
        
        // Choose a random spawn point
        Transform[] spawnPoints = persistentPlayerManager.spawnPoints;
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[spawnIndex].position;

        // Reset health and show UI
        currentHealth = maxHealth;
        ingameUI.setHealth(playerInput.playerIndex, 1);

        // Reactivate and blink
        gameObject.SetActive(true);
        StartCoroutine(BlinkSprite(1f)); // 1 second blink effect

        // Reset state
        if (fighterState != null)
        {
            fighterState.start_action(FighterAction.Idle);
            fighterState.set_grounded(false);
            fighterState.force_facing(1);
        }

        // Reset physics
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 1f;
        }

        GetComponent<BaseFighter>().died = false;

    }
    
    private void toggleFighter(bool value)
    {
        // Disable the fighter's controls and components
        var fighter = GetComponent<BaseFighter>();
        if (fighter != null)
        {
            fighter.enabled = value; }

        // Disable the collider
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = value;
        }

        // Disable the rigidbody
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = value;
        }
    }

    private void SetSpriteRenderersVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = r.color;
            c.a = visible ? 1f : 0f;
            r.color = c;
        }
    }

    private IEnumerator BlinkSprite(float duration)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float time = 0f;
        float blinkInterval = 0.1f;
        bool faded = false;

        while (time < duration)
        {
            foreach (var r in renderers)
            {
                Color c = r.color;
                c.a = faded ? 1f : 0.4f;
                r.color = c;
            }

            faded = !faded;
            yield return new WaitForSeconds(blinkInterval);
            time += blinkInterval;
        }

        // Ensure final alpha is fully visible
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = 1f;
            r.color = c;
        }

    }




    public void GrantExtraLife()
    {
        currentLives = 1;
        ingameUI.changeStocks(playerInput.playerIndex, 4);

        qteUsed = true;
        currentHealth = maxHealth;
        ingameUI.setHealth(playerInput.playerIndex, currentHealth / maxHealth);
    }


    public void Die(GameObject killer = null)
    {
        // TODO: Handle the death of the fighter, such as disabling controls, playing death animation, etc.

        toggleFighter(false);
        SetSpriteRenderersVisible(false);
        List<PlayerInput> playersAlive = persistentPlayerManager.getAlivePlayers();
        Time.timeScale = 1f;

        // Check if there are still more than one fighter alive
        if (playersAlive.Count <= 1)
        {

            int winnerIndex = playersAlive.Count == 1 ? playersAlive[0].playerIndex : -1;
            persistentPlayerManager.getPlayers().ForEach(x => x.SwitchCurrentActionMap("UI"));

            var winUI = FindAnyObjectByType<WinGameUI>(FindObjectsInactive.Include);

            if (winUI != null)
            {
                winUI.ShowWinner(winnerIndex);
            }

        }
        else if (killer != null)
        {
            QTEManager.Instance.ResumeRound(gameObject, killer);
        }


    }

    public IEnumerator GetFinished(GameObject killer)
    {
        // Play cutscene
        var cutscene = FindAnyObjectByType<CutscenePlayer>(FindObjectsInactive.Include);
        cutscene.PlayCutscene(
            killer.GetComponentInChildren<SpriteRenderer>().color,
            GetComponentInChildren<SpriteRenderer>().color
        );

        // Option A: Wait for animation to finish
        yield return new WaitForSecondsRealtime(8.8f);

        // OR Option B: Wait fixed time
        // yield return new WaitForSecondsRealtime(8.75f);

        // Continue after cutscene
        Die(killer);
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMissingHealthPortion()
    {
        return (maxHealth - currentHealth) / (float)maxHealth;
    }
}
