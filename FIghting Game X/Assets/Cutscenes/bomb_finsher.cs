using UnityEngine;
using UnityEngine.UI;

public class bomb_finsher : Cutscene
{
    [SerializeField] private RawImage beingFinishedImage;

    public override float CutsceneDuration => 6f;

    public override void Instantiate(Color finisherColor, Color finishedColor)
    {
        beingFinishedImage.color = finishedColor;
    }
}
