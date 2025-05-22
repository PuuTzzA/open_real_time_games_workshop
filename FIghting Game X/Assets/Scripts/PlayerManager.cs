using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();

    [SerializeField] private List<Transform> startingPoints;
    
    private PlayerInputManager playerInputManager;
    
    void Awake()
    {
        playerInputManager = FindFirstObjectByType<PlayerInputManager>();
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
        players.Add(player);
        var playerCount = players.Count;
        
        player.transform.position = startingPoints[playerCount - 1].position;
    }
}
