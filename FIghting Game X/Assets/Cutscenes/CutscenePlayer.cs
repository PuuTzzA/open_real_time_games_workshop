using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CutscenePlayer : MonoBehaviour
{
    public enum CutsceneType
    {
        Hammer_finisher,
        Bomb_Finisher
    }

    [SerializeField]
    private GameObject[] cutscenes = new GameObject[System.Enum.GetValues(typeof(CutsceneType)).Length];

    public event System.Action OnCutsceneFinished;



    private GameObject currentlyPlaying;

    public void PlayCutscene(Color finisherColor, Color finishedColor, CutsceneType cutscene)
    {
        if (currentlyPlaying == null)
        {
            currentlyPlaying = Instantiate(cutscenes[(int)cutscene]);
            currentlyPlaying.GetComponent<Cutscene>().Instantiate(finisherColor, finishedColor);
            StartCoroutine(WaitAndEndCutscene());
        }
    }

    private IEnumerator WaitAndEndCutscene()
    {
        yield return new WaitForSecondsRealtime(currentlyPlaying.GetComponent<Cutscene>().CutsceneDuration);
        EndCutscene();
    }

    public void EndCutscene()
    {
        Destroy(currentlyPlaying);
        OnCutsceneFinished?.Invoke();
    }
}
