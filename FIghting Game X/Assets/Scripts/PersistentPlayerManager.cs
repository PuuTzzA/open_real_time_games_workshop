using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(PlayerInputManager))]
public class PersistentPlayerManager : MonoBehaviour
{
    private PlayerInputManager _pim;
    private List<PlayerInput> players;

    [Header("UI References")]
    [SerializeField] private GameObject defaultJoinScreen;

    [Header("CharacterController Setup")]
    public Transform[] spawnPoints;

    [SerializeField] private GameObject spawnPointsObject;
    public GameObject[] characterPrefabs;

    private void Awake()
    {
        _pim = GetComponent<PlayerInputManager>();
        players = new List<PlayerInput>();   
        
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(spawnPointsObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnEnable()
    {
        _pim.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        _pim.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        if (SceneManager.GetActiveScene().name != "CharacterSelection") return;
        
        // Keep the player alive across scenes
        DontDestroyOnLoad(player.gameObject);
        players.Add(player);

        // Hide "press X to join" if first player joins
        if (defaultJoinScreen != null)
            defaultJoinScreen.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CharacterController")
        {
            _pim.DisableJoining();
            StartCoroutine(SpawnAllPlayers());
        }
    }

    private IEnumerator SpawnAllPlayers()
    {
        yield return null;

        // Create list of (index, controlScheme, devices) before destroying old inputs
        var playerData = players.Select(p => new {
            Index = p.playerIndex,
            Choice = GameManager.PlayerChoices[p.playerIndex],
            Scheme = p.currentControlScheme,
            Devices = p.devices.ToArray()
        }).ToList();

        // Destroy all old player input objects
        foreach (var player in players)
        {
            // Unpair their devices to avoid double control
            foreach (var device in player.devices)
                InputUser.PerformPairingWithDevice(device, user: default);

            Destroy(player.gameObject);
        }

        players.Clear();

        // Instantiate new player prefabs with correct inputs
        for (int i = 0; i < playerData.Count; i++)
        {
            var data = playerData[i];

            if (data.Choice < 0 || data.Choice >= characterPrefabs.Length)
            {
                Debug.LogError($"Invalid character choice for player {data.Index}");
                continue;
            }

            var character = PlayerInput.Instantiate(
                characterPrefabs[data.Choice],
                playerIndex: data.Index,
                controlScheme: data.Scheme,
                pairWithDevices: data.Devices
            );

            character.transform.position = spawnPoints[i].position;
        }
    }


}
