using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class SplitPanel : MonoBehaviour
{
    // List of instances running
    public static List<SplitPanel> instances = new List<SplitPanel>();

    [Tooltip("Drag the top-level UI RectTransform here")]
    [SerializeField] RectTransform panelRoot;

    // Attributes of each instance
    Camera uiCam;
    int myIndex;
    
    // UI elements
    public TMP_Text join_message;

    void Awake()
    {
        instances.Add(this);
        uiCam = GetComponentInChildren<Camera>();
        LayoutAll();
    }

    void OnDestroy()
    {
        instances.Remove(this);
        LayoutAll();
    }

    static void LayoutAll()
    {
        int total = instances.Count;
        for (int i = 0; i < total; i++)
        {
            var sp = instances[i];

            if (total > 1)
            {
                sp.join_message.gameObject.SetActive(false);
            }
            
            sp.myIndex = i;
            var rect = CalculateRect(i, total);
            
            if (sp.uiCam != null)
                sp.uiCam.rect = rect;
        }
    }

    static Rect CalculateRect(int idx, int total)
    {
        switch (total)
        {
            case 1:
                return new Rect(0, 0, 1, 1);

            case 2:
                return new Rect(idx * 0.5f, 0, 0.5f, 1);

            case 3:
            {
                float w = 0.5f, h = 0.5f;
                if (idx == 0) return new Rect(0f, 0.5f, w, h);
                if (idx == 1) return new Rect(0.5f, 0.5f, w, h);
                return new Rect(0f, 0f, 1.0f, h);
            }
            default:
            {
                int cols = 2, rows = 2;
                float w = 1f / cols, h = 1f / rows;
                int x = idx % cols;
                int y = 1 - (idx / cols); // top row first
                return new Rect(x * w, y * h, w, h);
            }
        }
    }

}
