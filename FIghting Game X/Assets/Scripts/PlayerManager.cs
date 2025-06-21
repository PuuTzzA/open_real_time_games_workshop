using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();

    [SerializeField] private List<Transform> startingPoints;

    [SerializeField] private List<GameObject> characterPrefabs;
    
    private PlayerInputManager playerInputManager;
    
    void Awake()
    {
        playerInputManager = FindFirstObjectByType<PlayerInputManager>();
    }

    private void Start()
    {
        for (int i = 0; i < GameManager.PlayerChoices.Count; i++)
        {
            playerInputManager.JoinPlayer(i);
        }
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }
    
    private void AddPlayer(PlayerInput player)
    {
        int index = player.playerIndex;
        int choice = GameManager.PlayerChoices[index];
        
        // Instantiate the chosen character
        var go = Instantiate(characterPrefabs[choice],
            startingPoints[index].position,
            Quaternion.identity);
        
        // Make the PlayerInput child of the character prefab
        player.transform.SetParent(go.transform, false);
    }
}
