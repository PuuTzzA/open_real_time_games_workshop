using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CharacterSelector : MonoBehaviour
{

    string[] highlightClasses = {
    "button-highlight-player0",
    "button-highlight-player1",
    "button-highlight-player2",
    "button-highlight-player3"
};

    private UIDocument uiDocument;
    public string playerHighlightClass = "button-highlight-player1"; // Or "button-highlight-player2"
    private int selectedIndex = 0;

    private VisualElement root;
    private Button[] characterButtons;
    public PlayerInput input;

    void Start()
    {

        uiDocument = GameObject.Find("Ui").GetComponent<UIDocument>();

        playerHighlightClass = highlightClasses[Mathf.Clamp(input.playerIndex, 0, highlightClasses.Length - 1)];

        //input = GetComponent<PlayerInput>();
        root = uiDocument.rootVisualElement;

        root.Q<Button>("char1").text = "halloWelt";
        // Find all 4 buttons (assuming IDs "char0", "char1", etc.)
        characterButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var btn = root.Q<Button>($"char{i}");
            btn.text = "123";
            characterButtons[i] = btn;
            //btn.AddToClassList("button-normal");
        }

        UpdateHighlight();
    }

    void Update()
    {

        Vector2 nav = input.actions["Navigate"].ReadValue<Vector2>();

        if (nav.x > 0.5f)
        {
            MoveSelection(1);
        }
        else if (nav.x < -0.5f)
        {
            MoveSelection(-1);
        }

        if (input.actions["Submit"].triggered)
        {
        }
    }

    private void MoveSelection(int dir)
    {
        characterButtons[selectedIndex].RemoveFromClassList(playerHighlightClass);

        selectedIndex += dir;
        if (selectedIndex < 0) selectedIndex = characterButtons.Length - 1;
        if (selectedIndex >= characterButtons.Length) selectedIndex = 0;

        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        characterButtons[selectedIndex].text = "test";
        characterButtons[selectedIndex].AddToClassList(playerHighlightClass);
    }
}
