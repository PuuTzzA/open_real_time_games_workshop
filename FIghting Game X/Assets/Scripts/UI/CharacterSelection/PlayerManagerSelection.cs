using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerManagerSelection : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    
    // UI
    [Header("UI References")] [SerializeField]
    private GameObject defaultJoinScreen;
    
    
    void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
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
        defaultJoinScreen.gameObject.SetActive(false);
    }
}