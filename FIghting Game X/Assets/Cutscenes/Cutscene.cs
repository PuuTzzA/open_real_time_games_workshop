using UnityEngine;

public abstract class Cutscene : MonoBehaviour
{
    public abstract void Instantiate(Color finishercolor, Color finishedcolor);
    public abstract float CutsceneDuration { get; }


}
