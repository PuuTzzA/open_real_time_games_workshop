using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;
using UnityEngine.InputSystem;

public class MinigameUI : MonoBehaviour
{

    /// <summary>
    /// SkillCheck
    /// </summary>
    public SkillCheck[] skillchecks = new SkillCheck[2];


    private VisualElement skillCheckSection;
    public int minigamenumber = 0;

    public GameObject killer;
    public GameObject fallen;

    /// <summary>
    /// Slide in Labels
    /// </summary>
    public float iconStartOffset = 0.3f;
    public float iconEndOffset = 0.3f;

    public SquareElement icon1;
    public SquareElement icon2;
    public float entryDuration = 1f;
    public float pauseDuration = 1f;
    public float exitDuration = 1f;
    public float offset = 500f;
    public float exitYOffset = 50f;
    public float exitXOffset = 100f; // new: horizontal movement
    public float finalScale = 0.5f; // Shrink to 50% over exit

    private Label vLabel;
    private Label sLabel;
    public Label player1;
    public Label player2;
    public ResponsiveLabel timer;

    private VisualElement root;


    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        vLabel = root.Q<Label>("V");
        sLabel = root.Q<Label>("S");
        player1 = root.Q<Label>("player1");
        player2 = root.Q<Label>("player2");
        icon1 = root.Q<SquareElement>("icon1");
        icon2 = root.Q<SquareElement>("icon2");
        skillCheckSection = root.Q<VisualElement>("MiniGame1");
        timer = root.Q<ResponsiveLabel>("timer");


        player1.style.color = fallen.GetComponent<BaseFighter>().playerColor;
        player2.style.color = killer.GetComponent<BaseFighter>().playerColor;
        player1.text = "PLAYER" + (fallen.GetComponent<PlayerInput>().playerIndex + 1);
        player2.text = "PLAYER" + (killer.GetComponent<PlayerInput>().playerIndex + 1);

        icon1.style.unityBackgroundImageTintColor = fallen.GetComponent<BaseFighter>().playerColor;
        icon2.style.unityBackgroundImageTintColor = killer.GetComponent<BaseFighter>().playerColor;




        for (int i = 0; i < skillchecks.Length; i++)
        {
            skillchecks[i] = (SkillCheck)root.Q<VisualElement>($"SkillCheck{i}").hierarchy.ElementAt(0).hierarchy.ElementAt(0);
        }



        StartCoroutine(SlideIcon(icon1, Vector2.left * offset, () => { timer.style.visibility = Visibility.Visible; if (minigamenumber == 1) { skillCheckSection.style.visibility = Visibility.Visible; minigamenumber = 0; } })); // enters from left, exits to left ✅
        StartCoroutine(SlideIcon(icon2, Vector2.right * offset)); // enters from right, exits to right ✅



        // 9° rotated entry/exit direction
        Vector2 rotatedUp = Quaternion.Euler(0, 0, 9) * Vector2.up;
        Vector2 rotatedDown = Quaternion.Euler(0, 0, 9) * Vector2.down;

        StartCoroutine(SlideThrough(vLabel, rotatedUp * offset, rotatedDown * offset));
        StartCoroutine(SlideThrough(sLabel, rotatedDown * offset, rotatedUp * offset));
        StartCoroutine(ShrinkAndMove(player1, -exitXOffset)); // move left
        StartCoroutine(ShrinkAndMove(player2, exitXOffset));  // move right
    }

    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (skillchecks[i].rotating)
            {
                skillchecks[i].ArrowAngle += skillchecks[i].spinSpeed * Time.unscaledDeltaTime;
                skillchecks[i].ArrowAngle %= 360f;
            }
        }
    }

    IEnumerator SlideIcon(VisualElement element, Vector2 direction, Action onComplete = null)
    {
        float iconExitDelay = pauseDuration + iconEndOffset + iconStartOffset;

        // Step 2: Slide In (from offset to center)
        yield return AnimateTranslate(element, direction, Vector2.zero, entryDuration, EaseOutCubic);

        // Step 3: Stay a bit longer than V/S
        yield return new WaitForSecondsRealtime(iconExitDelay);

        // Step 4: Slide Out (from center back to where it came from)
        yield return AnimateTranslate(element, Vector2.zero, direction, exitDuration, EaseInCubic);
        onComplete?.Invoke();
    }



    IEnumerator SlideThrough(VisualElement element, Vector2 entryOffset, Vector2 exitOffset)
    {
        yield return new WaitForSecondsRealtime(iconStartOffset);
        yield return AnimateTranslate(element, entryOffset, Vector2.zero, entryDuration, EaseOutCubic);
        yield return new WaitForSecondsRealtime(pauseDuration);
        yield return AnimateTranslate(element, Vector2.zero, exitOffset, exitDuration, EaseInCubic);

    }

    IEnumerator AnimateTranslate(VisualElement element, Vector2 from, Vector2 to, float duration, System.Func<float, float> easing)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easing(t);
            Vector2 current = Vector2.Lerp(from, to, easedT);
            element.style.translate = new StyleTranslate(new Translate(current.x, current.y, 0));
            yield return null;
        }

        element.style.translate = new StyleTranslate(new Translate(to.x, to.y, 0));
    }

    IEnumerator ShrinkAndMove(Label label, float horizontalOffset)
    {
        // Wait only for entry to finish
        yield return new WaitForSecondsRealtime(entryDuration);
        yield return new WaitForSecondsRealtime(pauseDuration);

        float totalAnimTime = exitDuration;
        float elapsed = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * finalScale;

        float startY = 0f;
        float endY = exitYOffset;

        float startX = 0f;
        float endX = horizontalOffset;

        while (elapsed < totalAnimTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / totalAnimTime);
            float easedT = EaseInCubic(t);

            // Scale
            Vector3 currentScale = Vector3.Lerp(startScale, endScale, easedT);
            label.transform.scale = currentScale;

            // Translate X & Y
            float currentX = Mathf.Lerp(startX, endX, easedT);
            float currentY = Mathf.Lerp(startY, endY, easedT);
            label.style.translate = new StyleTranslate(new Translate(currentX, currentY, 0));

            yield return null;
        }

        label.transform.scale = endScale;
        label.style.translate = new StyleTranslate(new Translate(endX, endY, 0));
    }


    float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
    float EaseInCubic(float t) => Mathf.Pow(Mathf.Clamp01(t), 3f);
}

