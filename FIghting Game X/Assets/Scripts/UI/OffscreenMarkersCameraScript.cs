using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#region Camera HUD script
[RequireComponent(typeof(Camera))]
public class OffscreenMarkersCameraScript : MonoBehaviour
{
    /*──────────────  Sizing & animation exposed in the inspector  ─────────────*/
    [Header("Sizing")]
    [Tooltip("Icon height in pixels when vertical resolution = 1080 px (Scale = 1).")]
    public float baseIconSize = 65f;

    [Tooltip("Arrow height as a factor of icon height.")]
    public float arrowToIcon = 1f;

    [Tooltip("Gap between icon centre and arrow centre (px @1080 p before scaling).")]
    public float arrowOffset = 0f;

    [Tooltip("Extra padding that keeps the arrow away from the screen edge "
           + "(px @1080 p, NOT affected by animation scale).")]                 // ★ NEW
    public float arrowEdgePadding = 20f;                                          // ★ NEW

    [Header("Animation")]
    [Tooltip("Seconds for the scale‑IN animation.")]
    public float enterDuration = 0.1f;

    [Tooltip("Seconds for the scale‑OUT animation.")]
    public float exitDuration = 0.1f;

    [Tooltip("Ease curve for 0→1 scale (played backwards to exit).")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    /*──────────────────────────────────────────────────────────────────────────*/

    /*────────────────── singleton helper ──────────────────*/
    private static OffscreenMarkersCameraScript _instance;
    public static OffscreenMarkersCameraScript Instance()
    {
        if (_instance) return _instance;
        var cam = Camera.main;
        if (!cam)
        {
            Debug.LogWarning("Off‑screen markers: no Camera.main in scene.");
            return null;
        }
        _instance = cam.GetComponent<OffscreenMarkersCameraScript>()
               ?? cam.gameObject.AddComponent<OffscreenMarkersCameraScript>();
        return _instance;
    }

    /*────────────────────────  internals  ────────────────────────*/
    Camera _cam;
    Camera Cam => _cam ??= GetComponent<Camera>();

    private struct State { public float progress; }
    readonly Dictionary<OffscreenMarker, State> _states = new();

    /*──────────────────  Registration helpers  ──────────────────*/
    public void Register(OffscreenMarker m)
    {
        if (!_states.ContainsKey(m))
            _states[m] = new State { progress = 0f };
    }
    public void Unregister(OffscreenMarker m) => _states.Remove(m);

    /*──────────────────────  Visibility helpers  ───────────────────────*/
    bool IsOnScreen(Vector3 worldPos)
    {
        var vp = Cam.WorldToViewportPoint(worldPos);
        return vp.z > 0f && vp.x is > 0f and < 1f && vp.y is > 0f and < 1f;
    }

    Vector3 FlipBehindCamera(Vector3 p)
    {
        Vector3 camToP = p - Cam.transform.position;
        float dot = Vector3.Dot(camToP, Cam.transform.forward);
        return dot >= 0 ? p : p - 2f * dot * Cam.transform.forward;
    }

    public void ForceHide(OffscreenMarker m)
    {
        if (_states.TryGetValue(m, out var s))
            _states[m] = new State { progress = s.progress }; // keep current progress (for smooth exit)
    }

    public void ForceShow(OffscreenMarker m)
    {
        if (!_states.ContainsKey(m))
            _states[m] = new State { progress = 0f }; // add if missing
    }

    /*────────────────────────  State machine  ────────────────────────*/
    void LateUpdate()
    {
        var keys = _states.Keys.ToList();
        foreach (var m in keys)
        {
            if (!m) { _states.Remove(m); continue; }

            bool offScreen = !IsOnScreen(m.transform.position);
            bool shouldShow = offScreen && !m.IsManuallyHidden();

            var s = _states[m];
            float target = shouldShow ? 1f : 0f;
            float duration = shouldShow ? enterDuration : exitDuration;
            float speed = duration <= 0.0001f ? float.PositiveInfinity : 1f / duration;

            s.progress = Mathf.MoveTowards(s.progress, target, Time.deltaTime * speed);
            _states[m] = s;
        }
    }

    /*────────────────────────────  Draw  ────────────────────────────*/
    void OnGUI()
    {
        var screenRect = Cam.pixelRect;
        float resScale = screenRect.height / 1080f;            // resolution‑independent pixels

        foreach (var kv in _states)
        {
            var m = kv.Key;
            var st = kv.Value;
            if (!m || st.progress <= 0.001f) continue;

            float s = ease.Evaluate(st.progress) * m.Scale;

            /*── keep original aspect ratios ─*/
            float iconH = baseIconSize * resScale * s;
            float arrowH = iconH * arrowToIcon;

            float iconAspect = m.Icon ? (float)m.Icon.width / m.Icon.height : 1f;
            float arrowAspect = m.Arrow ? (float)m.Arrow.width / m.Arrow.height : 1f;

            float iconW = iconH * iconAspect;
            float arrowW = arrowH * arrowAspect;

            Vector2 iExt = new(iconW * 0.5f, iconH * 0.5f);
            Vector2 aExt = new(arrowW * 0.5f, arrowH * 0.5f);

            /*── clamp icon centre so arrow stays on‑screen ─*/
            Vector3 wp = FlipBehindCamera(m.transform.position);
            Vector2 scr = Cam.WorldToScreenPoint(wp);
            scr.y = screenRect.height - scr.y;

            float gapToIcon = arrowOffset * resScale * s;                           // icon→arrow
            float edgePad = arrowEdgePadding * resScale;                          // ★ NEW
            float margin = arrowH + gapToIcon + edgePad;                         // ★ CHANGED

            Vector2 iPos = new(
                Mathf.Clamp(scr.x, iExt.x + margin, screenRect.width - iExt.x - margin),
                Mathf.Clamp(scr.y, iExt.y + margin, screenRect.height - iExt.y - margin));

            /*── ICON ─*/
            if (m.Icon)
                GUI.DrawTexture(
                    new Rect(iPos.x - iExt.x, iPos.y - iExt.y, iconW, iconH),
                    m.Icon, ScaleMode.StretchToFill, true, 0f, m.Color, 0, 0);

            /*── ARROW ─*/
            if (m.Arrow)
            {
                Vector2 toTarget = scr - iPos;
                if (toTarget.sqrMagnitude > .0001f)
                {
                    Vector2 dir = toTarget.normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

                    float gap = iExt.x + aExt.y + gapToIcon;
                    Vector2 aPos = iPos + dir * gap;

                    Matrix4x4 old = GUI.matrix;
                    GUIUtility.RotateAroundPivot(angle, aPos);

                    GUI.DrawTexture(
                        new Rect(aPos.x - aExt.x, aPos.y - aExt.y, arrowW, arrowH),
                        m.Arrow, ScaleMode.StretchToFill, true, 0f, m.Color, 0, 0);

                    GUI.matrix = old;
                }
            }
        }
    }
}
#endregion