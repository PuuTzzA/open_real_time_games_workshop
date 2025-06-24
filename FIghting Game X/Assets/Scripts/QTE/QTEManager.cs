using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QTEManager : MonoBehaviour
{
    
    public static QTEManager Instance { get; private set; }
    
    [Header("Minigame Prefabs")]
    [SerializeField] private GameObject mirrorSequenceUIPrefab;
    [SerializeField] private GameObject buttonSmashUIPrefab;
    [SerializeField] private GameObject tapTimingUIPrefab;

    private PlayerInput p1Input;
    private PlayerInput p2Input;
    private List<MonoBehaviour> pausedComponents = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartQTE(GameObject fallen, GameObject killer)
    {
        PauseAllExcept(fallen, killer);
        
        p1Input = fallen.GetComponent<PlayerInput>();
        p2Input = killer.GetComponent<PlayerInput>();
        // Pick a random int between 0 and 2
        int qteType = 1;

        switch (qteType)
        {
            case 0:
                MirrorSequenceMinigame(fallen, killer);
                break;
            case 1:
                ButtonSmashMinigame(fallen, killer);
                break;
            default:
                TapTimingMinigame(fallen, killer);
                break;
        }
        
    }

    private void EvaluateDualQTE(GameObject fallen, GameObject killer, QTEResult fallenResult, QTEResult killerResult)
    {
        bool isTie = fallenResult.IsSuccess && killerResult.IsSuccess ||
                     (!fallenResult.IsSuccess && !killerResult.IsSuccess && 
                      fallenResult.Score == killerResult.Score);
        
        if (isTie)
        {
            // Handle tie situation, e.g., both players get a second chance
            Debug.Log("It's a tie! Both players get a second chance.");
            ResumeRound(fallen, killer);
            return;
        }
        
        bool fallenWins = fallenResult.IsSuccess && !killerResult.IsSuccess || fallenResult.Score > killerResult.Score;

        var fallenHealth = fallen.GetComponent<FighterHealth>();
        if (fallenWins)
        {
            fallenHealth.GrantExtraLife();
            ResumeRound(fallen, killer);
        }
        else
        {
            fallenHealth.GetFinished(killer);
        }
    }
    
    private void MirrorSequenceMinigame(GameObject fallen, GameObject killer)
    {
        // Implement the Mirror Sequence Minigame logic here
        // This could involve showing a sequence of inputs that the player must replicate
    }
    
    private void ButtonSmashMinigame(GameObject fallen, GameObject killer)
    {
        // Implement the Button Smash Minigame logic here
        // This could involve a timed button mashing challenge
        var ui = Instantiate(buttonSmashUIPrefab);
        var qteUI = ui.GetComponent<ButtonSmashQTE>();
        
        // Initialize the QTE UI with player inputs and define the callback to evaluate results after finishing
        qteUI.Init(p1Input, p2Input, (fallenResult, killerResult) =>
        {
            EvaluateDualQTE(fallen, killer, fallenResult, killerResult);
        });
    }
    
    private void TapTimingMinigame(GameObject fallen, GameObject killer)
    {
        // Implement the Tap Timing Minigame logic here
        // This could involve tapping a button at the right time to score points
    }

    private void ResumeRound(GameObject fallen, GameObject killer)
    {
        p1Input.SwitchCurrentActionMap("Player");
        p2Input.SwitchCurrentActionMap("Player");
        
        ResumeAll();
    }

    private void PauseAllExcept(GameObject fallen, GameObject killer)
    {
        pausedComponents.Clear();

        foreach (var fighter in FindObjectsByType<BaseFighter>(FindObjectsSortMode.None))
        {
            if (fighter.gameObject == fallen || fighter.gameObject == killer)
                continue;

            fighter.enabled = false;
            pausedComponents.Add(fighter);
        }

        Time.timeScale = 0f;
    }

    private void ResumeAll()
    {
        foreach (var comp in pausedComponents)
        {
            if (comp != null) comp.enabled = true;
        }
        pausedComponents.Clear();
        
        Time.timeScale = 1f;
    }

}

public struct QTEResult
{
    public bool IsSuccess;   // e.g. hit the timing window
    public int Score;      // e.g. mash-count or timeRemaining
}