using UnityEngine;
using UnityEngine.UIElements;

public class RemoveElementOnKey : MonoBehaviour
{
    private VisualElement root;
    private int removeIndex = 0;
    public IngameUI ui;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && root != null)
        {
            ui.setNewHealth(0, 0.5f);
        }
         if (Input.GetKeyDown(KeyCode.Q) && root != null)
        {
            ui.setHealth(0, 1f);
        }
    }
}
