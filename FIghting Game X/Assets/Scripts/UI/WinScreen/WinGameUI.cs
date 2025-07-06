using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
// ReSharper disable InconsistentNaming

public class WinGameUI : MonoBehaviour
{
    [SerializeField] private UIDocument menuDocument;
    private VisualElement root;
    [SerializeField] private InputActionAsset inputActions;
    private void Awake()
    {
        root = menuDocument.rootVisualElement;
        root.Query<Button>(className: "endscreen_option_button").
            ForEach(option =>
            {
                option.RegisterCallback<ClickEvent>(OnOptionClicked);
            });
        
    }
    
    public void ShowWinner(int winnerIndex)
    {
        this.gameObject.SetActive(true);
        FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include).gameObject.SetActive(false);
        var winnerText = "";
        if (winnerIndex < 0)
        {
            winnerText = "No Winner! What a Situation!";
        }
        else
        {
            winnerText = "PLAYER " + (winnerIndex + 1) + " Wins!";
        }
        root.Q<Label>("Winner_Nomination").text = winnerText;
        root.Q<Button>("Rematch_Button").Focus();
        inputActions.FindAction("Confirm").performed += OnConfirmPressed;
    }

    private void OnConfirmPressed(InputAction.CallbackContext obj)
    {
        ChooseOption(root.focusController.focusedElement as Button);
    }

    private void ShowWinner()
    { 
        var persistentPlayerManager
            = FindFirstObjectByType<PersistentPlayerManager>().GetComponent<PersistentPlayerManager>();
        var playersAlive = persistentPlayerManager.getAlivePlayers();


        var winnerIndex = playersAlive.Count == 1 ? playersAlive[0].playerIndex : -1;
        persistentPlayerManager.getPlayers().ForEach(x => x.SwitchCurrentActionMap("UI"));
        FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include).gameObject.SetActive(false);
        var winnerText = "";
        if (winnerIndex < 0)
        {
            winnerText = "No Winner! What a Situation!";
        }
        else
        {
            winnerText = "PLAYER " + (winnerIndex + 1) + " Wins!";
        }
        root.Q<Label>("Winner_Nomination").text = winnerText;
    }

    private void OnOptionClicked(ClickEvent evt)
    {
       ChooseOption(evt.target as Button);
    }

    private void ChooseOption(Button option)
    {
        switch (option.name)
        {
            case "Rematch_Button":
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case "Character_Selection_Button":
                Destroy(GameObject.Find("SpawnPointsMapping"));
                SceneManager.LoadScene("Scenes/CharacterSelection");
                break;
            case "Return_To_Menu_Button":
                SceneManager.LoadScene("Scenes/MainMenu");
                break;
            default:
                // ReSharper disable once NotResolvedInText
                throw new ArgumentOutOfRangeException("WTF IS HAPPENED?");
                break;
        }
    }
    private void OnDestroy()
    {
        inputActions.FindAction("Confirm").performed -= OnConfirmPressed;
    }
}