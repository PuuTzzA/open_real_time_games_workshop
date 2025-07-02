using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TapTimingQTE : MonoBehaviour, IQTE
{
    [Header("UI Elements")]
    public Slider p1Slider;
    public Slider p2Slider;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI p1NameText;
    public TextMeshProUGUI p2NameText;
    public GameObject finishHimImageObject;
    
    [Header("Audio")]
    public AudioSource finishHimAudio;

    private float p1Taps = 0;
    private float p2Taps = 0;

    private float p1VisualValue = 0;
    private float p2VisualValue = 0;

    private float duration = 3f;
    private Action<QTEResult, QTEResult> onFinished;

    private InputAction p1Smash;
    private InputAction p2Smash;

    // Constants
    public float TAP_BOOST = 8f;
    public float GRAVITY = 50f; // Downforce per second
    public float SMOOTHNESS = 150f; // Higher is snappier

    public void Init(PlayerInput p1, PlayerInput p2, Action<QTEResult, QTEResult> callback)
    {
        onFinished = callback;
        p1NameText.text = "Player " + p1.playerIndex;
        p2NameText.text = "Player " + p2.playerIndex;

        p1Taps = 0;
        p2Taps = 0;

        p1Slider.minValue = 0;
        p1Slider.maxValue = 100;
        p2Slider.minValue = 0;
        p2Slider.maxValue = 100;

        // Switch to QTE map
        p1.SwitchCurrentActionMap("QTE");
        p2.SwitchCurrentActionMap("QTE");

        p1Smash = p1.actions["Smash"];
        p2Smash = p2.actions["Smash"];

        p1Smash.performed += OnP1Smash;
        p2Smash.performed += OnP2Smash;

        p1Smash.Enable();
        p2Smash.Enable();

        StartCoroutine(QTERoutine());
    }

    private void OnP1Smash(InputAction.CallbackContext ctx)
    {
        p1Taps += TAP_BOOST;
        p1Taps = Mathf.Clamp(p1Taps, 0f, 100f);
    }

    private void OnP2Smash(InputAction.CallbackContext ctx)
    {
        p2Taps += TAP_BOOST;
        p2Taps = Mathf.Clamp(p2Taps, 0f, 100f);
    }
    
    private IEnumerator QTERoutine()
    {
        float timeLeft = duration;

        while (timeLeft > 0f &&
               p1Taps < 100f &&
               p2Taps < 100f)
        {
            float dt = Time.unscaledDeltaTime;
            timeLeft -= dt;
            timerText.text = timeLeft.ToString("F1");

            // Apply downforce (gravity)
            p1Taps -= GRAVITY * dt;
            p2Taps -= GRAVITY * dt;
            p1Taps = Mathf.Clamp(p1Taps, 0f, 100f);
            p2Taps = Mathf.Clamp(p2Taps, 0f, 100f);

            float smoothingSpeed = SMOOTHNESS * dt;
            p1VisualValue = Mathf.MoveTowards(p1VisualValue, p1Taps, smoothingSpeed);
            p2VisualValue = Mathf.MoveTowards(p2VisualValue, p2Taps, smoothingSpeed);

            p1Slider.value = p1VisualValue;
            p2Slider.value = p2VisualValue;

            yield return null;
        }

        p1Smash.performed -= OnP1Smash;
        p2Smash.performed -= OnP2Smash;
        p1Smash.Disable();
        p2Smash.Disable();

        Destroy(gameObject);

        onFinished?.Invoke(
            new QTEResult { IsSuccess = p1Taps >= 100f, Score = Mathf.RoundToInt(p1Taps) },
            new QTEResult { IsSuccess = p2Taps >= 100f, Score = Mathf.RoundToInt(p2Taps) }
        );
    }
}
