using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinGameUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button characterSelectionButton;

    // Winner text tmpro
    [SerializeField] private TextMeshProUGUI winnerText;
    
    public void ShowWinner(int winnerIndex)
    {
        this.gameObject.SetActive(true);
        FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include).gameObject.SetActive(false);

        if (winnerIndex < 0)
        {
            winnerText.text = "No One Wins";
        }
        else
        {
            winnerText.text = "Player " + (winnerIndex + 1) + " Wins!";
        }
        
        characterSelectionButton.onClick.AddListener(() =>
        {
            Destroy(FindAnyObjectByType<PersistentPlayerManager>().GetComponent<PersistentPlayerManager>().gameObject);
            SceneManager.LoadScene("CharacterSelection");
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            Destroy(FindAnyObjectByType<PersistentPlayerManager>().GetComponent<PersistentPlayerManager>().gameObject);
            SceneManager.LoadScene("Potap_UI");
        });
    }
}