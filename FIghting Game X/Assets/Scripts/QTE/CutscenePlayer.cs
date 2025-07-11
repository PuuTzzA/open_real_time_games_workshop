using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CutscenePlayer : MonoBehaviour
{
    public event System.Action OnCutsceneFinished;

    [Header("Cutscene Setup")]
    public GameObject cutsceneRoot; // Parent object containing the animators

    [SerializeField] private RawImage beingFinishedImage;

    [SerializeField] private RawImage finisherImage;

    public float cutsceneDuration = 8.75f; // seconds

    private List<GameObject> reenableObjects = new List<GameObject>();

    /// <summary>
    /// Starts the cutscene, setting sprite colors and playing animations.
    /// </summary>
    /// <param name="finisherColor">Color to apply to the finisher sprite</param>
    /// <param name="finishedColor">Color to apply to the finished sprite</param>
    public void PlayCutscene(Color finisherColor, Color finishedColor)
    {
        // Disable all root objects except the cutsceneRoot or its children
        //foreach (GameObject obj in SceneRootObjects())
        //{
        //   if (obj == null || obj == cutsceneRoot || cutsceneRoot.transform.IsChildOf(obj.transform))
        //      continue;

        // if (obj.activeInHierarchy)
        //{
        //   obj.SetActive(false);
        //    reenableObjects.Add(obj);
        //}
        //}

        // Set sprite colors
        finisherImage.color = finisherColor;
        beingFinishedImage.color = finishedColor;

        // Ensure cutsceneRoot is active
        cutsceneRoot.SetActive(true);

        // Play animations
        //finisherAnimator.Play(finisherAnimation, 0, 0f);
        //finishedAnimator.Play(finishedAnimation, 0, 0f);

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
        //foreach (var obj in reenableObjects)
        //{
        //if (obj != null)
        //obj.SetActive(true);
        //}

        //reenableObjects.Clear();
        cutsceneRoot.SetActive(false);
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
