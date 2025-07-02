using UnityEngine;
using UnityEngine.UIElements;

public class SkillCheck : VisualElement
{
    public new class UxmlFactory : UxmlFactory<SkillCheck, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription thicknessAttr = new UxmlFloatAttributeDescription { name = "thickness", defaultValue = 0.1f };
        UxmlFloatAttributeDescription circleScaleAttr = new UxmlFloatAttributeDescription { name = "circle-scale", defaultValue = 0.75f };
        UxmlColorAttributeDescription circleColorAttr = new UxmlColorAttributeDescription { name = "circle-color", defaultValue = Color.white };

        UxmlFloatAttributeDescription arrowAngleAttr = new UxmlFloatAttributeDescription { name = "arrow-angle", defaultValue = 0f };
        UxmlColorAttributeDescription arrowColorAttr = new UxmlColorAttributeDescription { name = "arrow-color", defaultValue = Color.red };
        UxmlFloatAttributeDescription arrowThicknessAttr = new UxmlFloatAttributeDescription { name = "arrow-thickness", defaultValue = 2f };
        UxmlFloatAttributeDescription arrowLengthAttr = new UxmlFloatAttributeDescription { name = "arrow-length", defaultValue = 0.75f };

        // Second arrow attrs
        UxmlFloatAttributeDescription arrow2AngleAttr = new UxmlFloatAttributeDescription { name = "arrow2-angle", defaultValue = 180f };
        UxmlColorAttributeDescription arrow2ColorAttr = new UxmlColorAttributeDescription { name = "arrow2-color", defaultValue = Color.blue };
        UxmlFloatAttributeDescription arrow2ThicknessAttr = new UxmlFloatAttributeDescription { name = "arrow2-thickness", defaultValue = 2f };
        UxmlFloatAttributeDescription arrow2LengthAttr = new UxmlFloatAttributeDescription { name = "arrow2-length", defaultValue = 0.75f };

        // Second circle attrs
        UxmlFloatAttributeDescription circle2ThicknessAttr = new UxmlFloatAttributeDescription { name = "circle2-thickness", defaultValue = 0.05f };
        UxmlFloatAttributeDescription circle2FillPercentAttr = new UxmlFloatAttributeDescription { name = "circle2-fill-percent", defaultValue = 1f };
        UxmlColorAttributeDescription circle2ColorAttr = new UxmlColorAttributeDescription { name = "circle2-color", defaultValue = Color.gray };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var skillCheck = (SkillCheck)ve;

            skillCheck.ThicknessPercentage = Mathf.Clamp01(thicknessAttr.GetValueFromBag(bag, cc));
            skillCheck.CircleColor = circleColorAttr.GetValueFromBag(bag, cc);
            skillCheck.CircleScale = Mathf.Clamp01(circleScaleAttr.GetValueFromBag(bag, cc));

            skillCheck.ArrowAngle = arrowAngleAttr.GetValueFromBag(bag, cc);
            skillCheck.ArrowColor = arrowColorAttr.GetValueFromBag(bag, cc);
            skillCheck.ArrowThickness = arrowThicknessAttr.GetValueFromBag(bag, cc);
            skillCheck.ArrowLength = Mathf.Clamp01(arrowLengthAttr.GetValueFromBag(bag, cc));

            skillCheck.Arrow2Angle = arrow2AngleAttr.GetValueFromBag(bag, cc);
            skillCheck.Arrow2Color = arrow2ColorAttr.GetValueFromBag(bag, cc);
            skillCheck.Arrow2Thickness = arrow2ThicknessAttr.GetValueFromBag(bag, cc);
            skillCheck.Arrow2Length = Mathf.Clamp01(arrow2LengthAttr.GetValueFromBag(bag, cc));

            skillCheck.Circle2Thickness = Mathf.Clamp01(circle2ThicknessAttr.GetValueFromBag(bag, cc));
            skillCheck.Circle2FillPercent = Mathf.Clamp01(circle2FillPercentAttr.GetValueFromBag(bag, cc));
            skillCheck.Circle2Color = circle2ColorAttr.GetValueFromBag(bag, cc);
        }
    }

    // === Circle Properties ===
    private float thicknessPercentage = 0.1f;
    public float ThicknessPercentage
    {
        get => thicknessPercentage;
        set { thicknessPercentage = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    private float circleScale = 0.75f;
    public float CircleScale
    {
        get => circleScale;
        set { circleScale = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    private Color circleColor = Color.white;
    public Color CircleColor
    {
        get => circleColor;
        set { circleColor = value; MarkDirtyRepaint(); }
    }

    // === Arrow 1 Properties ===
    private float arrowAngle = 0f;
    public float ArrowAngle
    {
        get => arrowAngle;
        set { arrowAngle = value; MarkDirtyRepaint(); }
    }

    private Color arrowColor = Color.red;
    public Color ArrowColor
    {
        get => arrowColor;
        set { arrowColor = value; MarkDirtyRepaint(); }
    }

    private float arrowThickness = 2f;
    public float ArrowThickness
    {
        get => arrowThickness;
        set { arrowThickness = value; MarkDirtyRepaint(); }
    }

    private float arrowLength = 0.75f;
    public float ArrowLength
    {
        get => arrowLength;
        set { arrowLength = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    // === Arrow 2 Properties ===
    private float arrow2Angle = 180f;
    public float Arrow2Angle
    {
        get => arrow2Angle;
        set { arrow2Angle = value; MarkDirtyRepaint(); }
    }

    private Color arrow2Color = Color.blue;
    public Color Arrow2Color
    {
        get => arrow2Color;
        set { arrow2Color = value; MarkDirtyRepaint(); }
    }

    private float arrow2Thickness = 2f;
    public float Arrow2Thickness
    {
        get => arrow2Thickness;
        set { arrow2Thickness = value; MarkDirtyRepaint(); }
    }

    private float arrow2Length = 0.75f;
    public float Arrow2Length
    {
        get => arrow2Length;
        set { arrow2Length = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    // === Second circle properties ===
    private float circle2Thickness = 0.05f; // percentage of size (0..1)
    public float Circle2Thickness
    {
        get => circle2Thickness;
        set { circle2Thickness = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    private float circle2FillPercent = 1f; // 0..1, how much of the circle arc to draw
    public float Circle2FillPercent
    {
        get => circle2FillPercent;
        set { circle2FillPercent = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    private Color circle2Color = Color.gray;
    public Color Circle2Color
    {
        get => circle2Color;
        set { circle2Color = value; MarkDirtyRepaint(); }
    }

    public SkillCheck()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;

        float size = Mathf.Min(resolvedStyle.width, resolvedStyle.height);
        Vector2 center = new Vector2(resolvedStyle.width / 2, resolvedStyle.height / 2);
        float baseRadius = size * 0.5f * circleScale;
        float thickness = size * ThicknessPercentage * circleScale;

        float circle2ThicknessAbs = size * circle2Thickness;

        // --- Draw First Circle (below) ---
        {
            int segments = 128;
            float angleStep = 360f / segments;

            painter.strokeColor = circleColor;
            painter.lineWidth = thickness;

            painter.BeginPath();
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                float r = baseRadius; // centered stroke
                float x = center.x + Mathf.Cos(angle) * r;
                float y = center.y + Mathf.Sin(angle) * r;

                if (i == 0)
                    painter.MoveTo(new Vector2(x, y));
                else
                    painter.LineTo(new Vector2(x, y));
            }
            painter.ClosePath();
            painter.Stroke();
        }



        // --- Draw Second Circle (on top) ---
        if (circle2FillPercent > 0f && circle2Thickness > 0f)
        {
            int segments = 128;
            float totalAngle = 360f * circle2FillPercent;
            float angleStep = totalAngle / segments;

            // Increase radius so second circle overextends equally on both sides of first circle:
            // Add half thickness of first circle and half thickness of second circle to baseRadius
            float circle2Radius = baseRadius;

            painter.strokeColor = circle2Color;
            painter.lineWidth = circle2ThicknessAbs;

            painter.BeginPath();

            for (int i = 0; i <= segments; i++)
            {
                // Start at arrow2Angle, sweep clockwise by totalAngle
                float angle = Mathf.Deg2Rad * (arrow2Angle + i * angleStep);

                float x = center.x + Mathf.Cos(angle) * circle2Radius;
                float y = center.y + Mathf.Sin(angle) * circle2Radius;

                if (i == 0)
                    painter.MoveTo(new Vector2(x, y));
                else
                    painter.LineTo(new Vector2(x, y));
            }

            painter.Stroke();
        }
        // --- Draw Arrows ---
        DrawArrow2(painter, center, baseRadius, arrow2Angle, arrow2Color, arrow2Thickness, arrow2Length);
        DrawArrow1(painter, center, size, arrowAngle, arrowColor, arrowThickness, arrowLength);
    }


    private void DrawArrow1(Painter2D painter, Vector2 center, float size, float angleDegrees, Color color, float thickness, float lengthPercent)
    {
        float arrowRadians = Mathf.Deg2Rad * angleDegrees;
        float outerRadius = size * 0.5f;
        float arrowLengthAbs = lengthPercent * outerRadius;

        Vector2 dir = new Vector2(Mathf.Cos(arrowRadians), Mathf.Sin(arrowRadians));
        Vector2 perp = new Vector2(-dir.y, dir.x);

        // Tip always on the edge
        Vector2 tip = center + dir * outerRadius;
        // Tail moves inward by arrowLengthAbs from tip towards center
        Vector2 tail = center + dir * (outerRadius - arrowLengthAbs);

        // thickness at tail and tip
        float baseThickness = thickness * 0.5f;
        float tipThickness = thickness;

        Vector2 tailLeft = tail - perp * baseThickness;
        Vector2 tailRight = tail + perp * baseThickness;
        Vector2 tipLeft = tip - perp * tipThickness;
        Vector2 tipRight = tip + perp * tipThickness;

        painter.fillColor = color;

        painter.BeginPath();
        painter.MoveTo(tailLeft);
        painter.LineTo(tipLeft);
        painter.LineTo(tipRight);
        painter.LineTo(tailRight);
        painter.ClosePath();
        painter.Fill();
    }

    private void DrawArrow2(Painter2D painter, Vector2 center, float radius, float angleDegrees, Color color, float thickness, float lengthPercent)
    {
        float arrowRadians = Mathf.Deg2Rad * angleDegrees;
        float arrowLengthAbs = lengthPercent * radius;

        Vector2 dir = new Vector2(Mathf.Cos(arrowRadians), Mathf.Sin(arrowRadians));
        Vector2 perp = new Vector2(-dir.y, dir.x);

        float halfLength = arrowLengthAbs * 0.5f;

        Vector2 tail = center + dir * (radius - halfLength);
        Vector2 tip = center + dir * (radius + halfLength);

        float baseThickness = thickness * 0.5f;
        float tipThickness = thickness;

        Vector2 tailLeft = tail - perp * baseThickness;
        Vector2 tailRight = tail + perp * baseThickness;
        Vector2 tipLeft = tip - perp * tipThickness;
        Vector2 tipRight = tip + perp * tipThickness;

        painter.fillColor = color;

        painter.BeginPath();
        painter.MoveTo(tailLeft);
        painter.LineTo(tipLeft);
        painter.LineTo(tipRight);
        painter.LineTo(tailRight);
        painter.ClosePath();
        painter.Fill();
    }

}
