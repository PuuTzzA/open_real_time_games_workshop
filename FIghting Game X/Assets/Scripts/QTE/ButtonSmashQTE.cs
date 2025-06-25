using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ButtonSmashQTE : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider p1Slider;
    public Slider p2Slider;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI p1NameText;
    public TMPro.TextMeshProUGUI p2NameText;

    private int p1Taps;
    private int p2Taps;

    private float duration = 3f;
    private Action<QTEResult, QTEResult> onFinished;

    private InputAction p1Smash;
    private InputAction p2Smash;

    public void Init(PlayerInput p1, PlayerInput p2, Action<QTEResult, QTEResult> callback)
    {
        onFinished = callback;
        p1NameText.text = "Player " + p1.playerIndex;
        p2NameText.text = "Player " + p2.playerIndex;
        p1Taps = 0;
        p2Taps = 0;

        // Temporarily switch both to QTE map
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
        p1Taps++;
        p1Slider.value = p1Taps;
    }

    private void OnP2Smash(InputAction.CallbackContext ctx)
    {
        p2Taps++;
        p2Slider.value = p2Taps;
    }

    private IEnumerator QTERoutine()
    {
        float timeLeft = duration;
        timerText.text = duration.ToString("F1");

        while (timeLeft > 0f && 
               p1Taps < 10 && 
               p2Taps < 10)
        {
            timeLeft -= Time.unscaledDeltaTime;
            timerText.text = timeLeft.ToString("F1");
            yield return null;
        }

        // Detach and restore
        p1Smash.performed -= OnP1Smash;
        p2Smash.performed -= OnP2Smash;
        p1Smash.Disable();
        p2Smash.Disable();

        Destroy(gameObject);

        onFinished?.Invoke(
            new QTEResult { IsSuccess = p1Taps >= 10, Score = p1Taps },
            new QTEResult { IsSuccess = p2Taps >= 10, Score = p2Taps }
        );
    }
}