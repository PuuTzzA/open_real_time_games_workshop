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

    public void TakeDamage(int dmg, GameObject attacker, bool bomb = false)
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
            StartCoroutine(HandleDeath(attacker, bomb));
        }
    }


    private IEnumerator HandleDeath(GameObject killer, bool bomb)
    {
        currentLives--;
        ingameUI.changeStocks(playerInput.playerIndex, currentLives);
        fighterState.start_action(FighterAction.Death);
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Debug.Log("death");


        if (currentLives <= 0)
        {
            if (!qteUsed && killer != null && killer != gameObject)
            {
                qteUsed = true;
                yield return DeferQTEStart(killer,bomb);
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

    private IEnumerator DeferQTEStart(GameObject killer, bool bomb)
    {
        yield return new WaitForEndOfFrame();
        bool qteFinished = false;
        QTEManager.Instance.StartQTE(this.gameObject, killer, bomb, () =>
        {
            qteFinished = true;
        });

        while (!qteFinished)
        {
            yield return null; // Wait until QTE is finished
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
        Debug.Log("respawn");



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




    public void GrantExtraLife(int health)
    {
        currentLives = 1;
        ingameUI.changeStocks(playerInput.playerIndex, 4);

        qteUsed = true;
        currentHealth = health;
        ingameUI.setHealth(playerInput.playerIndex, currentHealth / maxHealth);

        gameObject.GetComponent<BaseFighter>().next_idle_action();
    }


    public void Die(GameObject killer = null)
    {
        // TODO: Handle the death of the fighter, such as disabling controls, playing death animation, etc.


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

    public IEnumerator GetFinished(GameObject killer, bool bomb)
    {
        var cutscene = FindAnyObjectByType<CutscenePlayer>(FindObjectsInactive.Include);

        bool cutsceneDone = false;

        // Subscribe to event
        cutscene.OnCutsceneFinished += () => cutsceneDone = true;

        // Play the cutscene
        cutscene.PlayCutscene(
            killer.GetComponentInChildren<BaseFighter>().playerColor,
            GetComponentInChildren<BaseFighter>().playerColor,
            bomb ? CutscenePlayer.CutsceneType.Bomb_Finisher : CutscenePlayer.CutsceneType.Hammer_finisher
        );

        // Wait until cutscene is finished
        yield return new WaitUntil(() => cutsceneDone);

        // Optionally: Unsubscribe if needed (not necessary in this one-shot use case)
        // cutscene.OnCutsceneFinished -= ...;

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
