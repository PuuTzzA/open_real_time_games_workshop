using System.Collections;
using UnityEngine;

public class TestCutscene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        // Optionally start a test cutscene for demonstration
        StartCoroutine(TestCutScene());
    }

    private IEnumerator TestCutScene()
    {
        yield return new WaitForSecondsRealtime(5f);
        FindAnyObjectByType<CutscenePlayer>(FindObjectsInactive.Include).PlayCutscene(new Color(1f, 0.5f, 0.5f), new Color(0.5f, 1f, 0.5f));
    }
}
