using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Random = System.Random;

public class TapTimingQTE : MonoBehaviour, IQTE
{
    private MinigameUI ui;

    private int[] points = new int[2];
    private float[] angles = new float[3];

    private float spinSpeedmultiplier = 1.1f;
    [SerializeField]
    private float arrowstunn = 0.2f;

    [Header("Audio")]
    public AudioSource finishHimAudio;

    private float duration = 3f;
    private Action<QTEResult, QTEResult> onFinished;

    private InputAction p1Smash;
    private InputAction p2Smash;



    void OnEnable()
    {
        ui = this.gameObject.GetComponent<MinigameUI>();
        angles[0] = (float)(new Random().NextDouble() * (180f - 8f) + 8f);
        for (int i = 1; i < 3; i++)
        {
            ui.skillchecks[0].Arrow2Angle = angles[i - 1];
            angles[i] = ui.skillchecks[0].GetValidNewArrow2Angle();
        }


        ui.skillchecks[0].Arrow2Angle = angles[0];
        ui.skillchecks[1].Arrow2Angle = angles[0];
    }

    public void Init(PlayerInput p1, PlayerInput p2, Action<QTEResult, QTEResult> callback)
    {



        ui.skillchecks[0].rotating = true;
        ui.skillchecks[1].rotating = true;
        points[0] = 0;
        points[1] = 0;
        onFinished = callback;
        ui.player1.text = "Player " + p1.playerIndex;
        ui.player2.text = "Player " + p2.playerIndex;

        // Switch to QTE map
        p1.SwitchCurrentActionMap("QTE");
        p2.SwitchCurrentActionMap("QTE");

        p1Smash = p1.actions["Smash"];
        p2Smash = p2.actions["Smash"];

        p1Smash.performed += ctx => OnSmash(0);
        p2Smash.performed += ctx => OnSmash(1);

        p1Smash.Enable();
        p2Smash.Enable();

        StartCoroutine(QTERoutine());
    }

    private void OnSmash(int player)
    {
        int hitResult = ui.skillchecks[player].CheckArrowHit();

        switch (hitResult)
        {
            case 1:
                points[player]++;
                ui.skillchecks[player].spinSpeed *= -1;
                ui.skillchecks[player].Arrow2Angle = angles[points[player]];
                ui.skillchecks[player].Circle2FillPercent *= -1;
                break;
            case 2:
                points[player]++;
                ui.skillchecks[player].spinSpeed *= spinSpeedmultiplier;
                ui.skillchecks[player].spinSpeed *= -1;
                ui.skillchecks[player].Arrow2Angle = angles[points[player]];
                ui.skillchecks[player].Circle2FillPercent *= -1;
                break;
            default:
                ui.skillchecks[player].rotating = false;
                StartCoroutine(pauseArrow(player));
                if (player == 0)
                    p1Smash.Disable();
                else
                    p2Smash.Disable();
                break;
        }
    }

    private IEnumerator pauseArrow(int player)
    {
        yield return new WaitForSecondsRealtime(arrowstunn);
        ui.skillchecks[player].rotating = true;
        if (player == 0)
            p1Smash.Enable();
        else
            p2Smash.Enable();
        yield return null;
    }

    private IEnumerator QTERoutine()
    {

        float timeLeft = duration;

        while (timeLeft > 0f &&
               points[0] < 3 &&
               points[1] < 3)
        {
            float dt = Time.unscaledDeltaTime;
            timeLeft -= dt;
            ui.timer.text = timeLeft.ToString("F1");


            yield return null;
        }

        p1Smash.performed -= ctx => OnSmash(0);
        p2Smash.performed -= ctx => OnSmash(1);
        p1Smash.Disable();
        p2Smash.Disable();

        //Destroy(gameObject);

        onFinished?.Invoke(
            new QTEResult { IsSuccess = points[0] >= 3, Score = points[0] },
            new QTEResult { IsSuccess = points[1] >= 3, Score = points[1] }
        );
    }
}
