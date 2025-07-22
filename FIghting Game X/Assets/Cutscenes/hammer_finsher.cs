using UnityEngine;
using UnityEngine.UI;

public class hammer_finsher : Cutscene
{
    [SerializeField] private RawImage beingFinishedImage;
    [SerializeField] private RawImage finisherImage;


    public override float CutsceneDuration => 9.7f;

    public override void Instantiate(Color finisherColor, Color finishedColor)
    {
        finisherImage.color = finisherColor;
        beingFinishedImage.color = finishedColor;
    }
}
