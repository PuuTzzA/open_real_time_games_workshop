using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Off‑screen marker component
[DisallowMultipleComponent]
public class OffscreenMarker : MonoBehaviour
{
    [Header("Graphics")]
    public Texture Icon;                      // any aspect ratio
    public Texture Arrow;                     // must point up (+Y) in the texture
    public Color Color = Color.white;

    private bool manuallyHidden = false;

    [Header("Per‑marker settings")]
    [Tooltip("Extra scale multiplier for this marker (1 = default).")]
    public float Scale = 1f;

    void Awake() => OffscreenMarkersCameraScript.Instance()?.Register(this);
    void OnDestroy() => OffscreenMarkersCameraScript.Instance()?.Unregister(this);
    public void DisableIndicator()
    {
        manuallyHidden = true;
        OffscreenMarkersCameraScript.Instance()?.ForceHide(this);
    }

    public void EnableIndicator()
    {
        manuallyHidden = false;
        OffscreenMarkersCameraScript.Instance()?.ForceShow(this);
    }

    public bool IsManuallyHidden() => manuallyHidden;
}
#endregion