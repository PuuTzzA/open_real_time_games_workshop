using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutscenePlayer : MonoBehaviour
{

    public event System.Action OnCutsceneFinished;

    [Header("Cutscene Setup")]
    public GameObject cutsceneRoot; // Parent object containing the animators
    public Animator finisherAnimator;
    public Animator finishedAnimator;

    public SpriteRenderer finisherSprite;
    public SpriteRenderer finishedSprite;

    public string finisherAnimation = "finishing";
    public string finishedAnimation = "being_finished";
    public float cutsceneDuration = 8.75f; // seconds

    private List<GameObject> reenableObjects = new List<GameObject>();

    /// <summary>
    /// Starts the cutscene, setting sprite colors and playing animations.
    /// </summary>
    /// <param name="finisherColor">Color to apply to the finisher sprite</param>
    /// <param name="finishedColor">Color to apply to the finished sprite</param>
    public void PlayCutscene(Color finisherColor, Color finishedColor)
    {
        // Pause the game
        Time.timeScale = 0f;

        // Disable all root objects except the cutsceneRoot or its children
        foreach (GameObject obj in SceneRootObjects())
        {
            if (obj == null || obj == cutsceneRoot || cutsceneRoot.transform.IsChildOf(obj.transform))
                continue;

            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
                reenableObjects.Add(obj);
            }
        }

        // Ensure cutsceneRoot is active
        cutsceneRoot.SetActive(true);

        // Set sprite colors
        if (finisherSprite != null) finisherSprite.color = finisherColor;
        if (finishedSprite != null) finishedSprite.color = finishedColor;

        // Play animations
        finisherAnimator.Play(finisherAnimation, 0, 0f);
        finishedAnimator.Play(finishedAnimation, 0, 0f);

        // Start coroutine to end the cutscene
        StartCoroutine(WaitAndEndCutscene());
    }

    private IEnumerator WaitAndEndCutscene()
    {
        yield return new WaitForSecondsRealtime(cutsceneDuration);
        EndCutscene();
    }

    public void EndCutscene()
    {
        foreach (var obj in reenableObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        reenableObjects.Clear();
        Time.timeScale = 1f;

        OnCutsceneFinished?.Invoke(); // <-- Notify listeners
    }

    private List<GameObject> SceneRootObjects()
    {
        var roots = new List<GameObject>();
        var scene = gameObject.scene;
        scene.GetRootGameObjects(roots);
        return roots;
    }
}
