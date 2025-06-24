using UnityEngine;
using UnityEngine.UIElements;

public class RemoveElementOnKey : MonoBehaviour
{
    private VisualElement root;
    private int removeIndex = 0;

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
            string elementName = $"SquareElement{removeIndex}";
            var element = root.Q(elementName);

            if (element != null)
            {
                element.RemoveFromHierarchy();
                Debug.Log($"{elementName} removed.");
            }
            else
            {
                Debug.Log($"{elementName} not found.");
            }

            removeIndex++;
        }
    }
}
