using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class QTEManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ui;

    public static QTEManager Instance { get; private set; }

    [Header("Intro UI & SFX")]
    [SerializeField] private GameObject finishHimPanel;
    [SerializeField] private AudioClip finishHimVoice;
    private AudioSource finishHimAudioSource;

    [Header("Minigame Prefabs")]
    [SerializeField] private GameObject mirrorSequenceUIPrefab;
    [SerializeField] private GameObject buttonSmashUIPrefab;
    [SerializeField] private GameObject tapTimingUIPrefab;

    private PlayerInput p1Input;
    private PlayerInput p2Input;
    private List<MonoBehaviour> pausedComponents = new();
    private PersistentPlayerManager persistentPlayerManager;
    private IngameUI _ingameUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        finishHimAudioSource = GetComponent<AudioSource>();
        persistentPlayerManager = FindFirstObjectByType<PersistentPlayerManager>().GetComponent<PersistentPlayerManager>();
    }

    public void StartQTE(GameObject fallen, GameObject killer, Action onDone)
    {
        PauseAllExcept(fallen, killer);
        _ingameUI = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
        _ingameUI.gameObject.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.None;

        p1Input = fallen.GetComponent<PlayerInput>();
        p2Input = killer.GetComponent<PlayerInput>();
        // Pick a random int between 0 and 2
        StartCoroutine(StartQTESequence(fallen, killer, onDone));
    }

    private IEnumerator StartQTESequence(GameObject fallen, GameObject killer, Action onDone)
    {
        int type = UnityEngine.Random.Range(0, 2); // Randomly choose between 0, 1
        Debug.Log(type);
        if (type == 1)
            ui.GetComponent<MinigameUI>().minigamenumber = 1;


        // 1) Show “Finish Him” intro
        finishHimPanel.SetActive(true);

        var minigameUI = finishHimPanel.GetComponent<MinigameUI>();

        float totalDuration = minigameUI.entryDuration + minigameUI.pauseDuration + minigameUI.exitDuration + minigameUI.iconEndOffset + minigameUI.iconStartOffset;

        if (finishHimVoice != null) finishHimAudioSource.PlayOneShot(finishHimVoice);

        // (optional) trigger an Animator on finishHimPanel here
        // var anim = finishHimPanel.GetComponent<Animator>();
        // anim.SetTrigger("PopIn");

        // wait one real-time second (unscaled)
        yield return new WaitForSecondsRealtime(totalDuration);

        // 2) Hide intro

        // 3) Now start the actual minigame
        StartCoroutine(SpawnMinigame(type, fallen, killer, onDone));
    }

    private IEnumerator SpawnMinigame(int type, GameObject fallen, GameObject killer, Action onDone)
    {
        // instantiate the correct UI and init it
        IQTE qte;
        if (type == 0)
        {
            var prefab = Instantiate(buttonSmashUIPrefab);
            Canvas canvas = prefab.GetComponent<Canvas>();

            // Setup Canvas to render in front of UI Toolkit UI
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            qte = prefab.GetComponent<ButtonSmashQTE>();
        }
        else
        {
            // qte = prefab.GetComponent<TapTimingQTE>();
            qte = ui.GetComponent<TapTimingQTE>(); // For simplicity, using ButtonSmashQTE here
        }

        yield return new WaitForSecondsRealtime(0.5f); // Give some time for the UI to instantiate and show up

        qte.Init(p1Input, p2Input, (r1, r2) =>
        {
            EvaluateDualQTE(fallen, killer, r1, r2);
            onDone?.Invoke();
        });
    }

    private void EvaluateDualQTE(GameObject fallen, GameObject killer, QTEResult fallenResult, QTEResult killerResult)
    {
        Time.timeScale = 1f; 
        bool isTie = fallenResult.IsSuccess && killerResult.IsSuccess ||
                     (!fallenResult.IsSuccess && !killerResult.IsSuccess &&
                      fallenResult.Score == killerResult.Score);

        if (isTie)
        {
            // Handle tie situation, e.g., both players get a second chance
            ResumeRound(fallen, killer);
            return;
        }

        bool fallenWins = fallenResult.IsSuccess && !killerResult.IsSuccess || fallenResult.Score > killerResult.Score;

        var fallenHealth = fallen.GetComponent<FighterHealth>();
        if (fallenWins)
        {
            fallenHealth.GrantExtraLife();
        }
        else
        {
            bool isFinished = fallenHealth.GetFinished(killer);
            if (isFinished) return;
        }
        ResumeRound(fallen, killer);
    }

    private void MirrorSequenceMinigame(GameObject fallen, GameObject killer)
    {
        // Implement the Mirror Sequence Minigame logic here
        // This could involve showing a sequence of inputs that the player must replicate
    }

    private void ButtonSmashMinigame(GameObject fallen, GameObject killer)
    {
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
        List<PlayerInput> players = persistentPlayerManager.getPlayers();


        foreach (var fighter in players)
        {
            if (fighter.playerIndex == fallen.GetComponent<PlayerInput>().playerIndex || fighter.playerIndex == killer.GetComponent<PlayerInput>().playerIndex)
                continue;

            fighter.enabled = false;
            pausedComponents.Add(fighter);
        }

        Time.timeScale = 0f;
    }

    private void ResumeAll()
    {
        _ingameUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        foreach (var comp in pausedComponents)
        {
            if (comp != null) comp.enabled = true;
        }
        pausedComponents.Clear();
        finishHimPanel.SetActive(false);

        Time.timeScale = 1f;
    }

}

public struct QTEResult
{
    public bool IsSuccess;   // e.g. hit the timing window
    public int Score;      // e.g. mash-count or timeRemaining
}