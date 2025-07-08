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

    public Color[] availableColors;
    private static PersistentPlayerManager _instance;
    private PlayerInputManager _pim;
    private List<PlayerInput> players;

    [Header("UI References")]
    [SerializeField] private GameObject defaultJoinScreen;
    [SerializeField] private GameObject playerSelectionPrefab;

    [Header("CharacterController Setup")]
    public Transform[] spawnPoints;

    [SerializeField] private GameObject spawnPointsObject;
    public GameObject[] characterPrefabs;

    // For testing
    private string fightScene = "TestSceneMartin";

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (spawnPointsObject == null)
        {
            spawnPointsObject = GameObject.Find("SpawnPointsMapping");
        }

        _pim = GetComponent<PlayerInputManager>();
        players = new List<PlayerInput>();
        _pim.EnableJoining();

        foreach (var player in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
        {
            player.user.UnpairDevicesAndRemoveUser();
            player.DeactivateInput();
            Destroy(player.gameObject);
        }

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
        if (_pim == null) return;
        _pim.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        if (_pim == null) return;
        _pim.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        if (SceneManager.GetActiveScene().name != "CharacterSelection") return;

        // Check if player already exists

        foreach (var existingPlayer in players)
        {
            if (existingPlayer.playerIndex == player.playerIndex)
            {
                return;
            }
        }

        // Keep the player alive across scenes
        DontDestroyOnLoad(player.gameObject);
        players.Add(player);


        // Hide "press X to join" if first player joins
        if (defaultJoinScreen != null)
            defaultJoinScreen.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == fightScene)
        {
            _pim.DisableJoining();
            StartCoroutine(SpawnAllPlayers());
        }
        else if (scene.name == "CharacterSelection")
        {
            if (spawnPointsObject == null)
            {
                spawnPointsObject = GameObject.Find("SpawnPointsMapping");

                spawnPoints = spawnPointsObject.GetComponentsInChildren<Transform>()
                    .ToArray();
                DontDestroyOnLoad(spawnPointsObject);
            }

            // Ensure spawn points are set up

            // 1) Switch back into join-mode
            _pim.EnableJoining();

            // 2) If we're coming from the fight, re-use that data:
            if (players.Count > 0)
            {
                // 2a) Capture index+scheme+devices from the fighter inputs
                var selectionData = players.Select(p => new
                {
                    PrefabChoice = GameManager.PlayerChoices[p.playerIndex],
                    ColorChoice = GameManager.PlayerColorChoices[p.playerIndex],
                    Index = p.playerIndex,
                    Scheme = p.currentControlScheme,
                    Devices = p.devices.ToArray()
                }).ToList();


                // 2b) Tear down the fighter inputs
                foreach (var player in players)
                {
                    // Unpair their devices to avoid double control
                    foreach (var device in player.devices)
                        InputUser.PerformPairingWithDevice(device, user: default);

                    player.DeactivateInput();
                    if (player.user.valid) player.user.UnpairDevicesAndRemoveUser();
                    Destroy(player.gameObject);
                }
                players.Clear();

                // 2c) Re-instantiate CharacterSelection panels
                foreach (var data in selectionData)
                {
                    var selection = PlayerInput.Instantiate(
                        playerSelectionPrefab,
                        playerIndex: data.Index,
                        controlScheme: data.Scheme,
                        pairWithDevices: data.Devices
                    );
                    selection.GetComponent<SelectionManager>().selectedCharacter = data.PrefabChoice;
                    selection.GetComponent<SelectionManager>().selectedColorIndex = data.ColorChoice;
                    DontDestroyOnLoad(selection.gameObject);
                    players.Add(selection);
                }

                // 2d) Hide the “press to join” prompt since we have existing players
                if (defaultJoinScreen != null)
                    defaultJoinScreen.SetActive(false);
            }
        }
    }

    private IEnumerator SpawnAllPlayers()
    {
        yield return null;

        // Create list of (index, controlScheme, devices) before destroying old inputs
        var playerData = players.Select(p => new
        {
            Index = p.playerIndex,
            PrefabChoice = GameManager.PlayerChoices[p.playerIndex],
            ColorChoice = GameManager.PlayerColorChoices[p.playerIndex],
            Scheme = p.currentControlScheme,
            Devices = p.devices.ToArray()
        }).ToList();


        // Destroy all old player input objects
        foreach (var player in players)
        {
            // Unpair their devices to avoid double control
            foreach (var device in player.devices)
                InputUser.PerformPairingWithDevice(device, user: default);

            player.DeactivateInput();

            if (player.user.valid) player.user.UnpairDevicesAndRemoveUser();
            Destroy(player.gameObject);
        }

        players.Clear();

        var usedDevices = new HashSet<InputDevice>();

        // Instantiate new player prefabs with correct inputs
        for (int i = 0; i < playerData.Count; i++)
        {
            var data = playerData[i];
            // Check if this data.Devices are already used
            if (data.Devices.Any(d => usedDevices.Contains(d)))
            {
                continue;
            }

            if (data.PrefabChoice < 0 || data.PrefabChoice >= characterPrefabs.Length)
                continue;

            var character = PlayerInput.Instantiate(
                characterPrefabs[data.PrefabChoice],
                playerIndex: data.Index,
                controlScheme: data.Scheme,
                pairWithDevices: data.Devices
            );
            character.GetComponentInChildren<SpriteRenderer>().color = availableColors[data.ColorChoice];

            players.Add(character);
            DontDestroyOnLoad(character);

            character.transform.position = spawnPoints[i].position;

            foreach (var d in data.Devices)
                usedDevices.Add(d);
        }


        IngameUI ui = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
        while (ui == null)
            ui = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
        ui.gameObject.SetActive(true);
    }

    public List<PlayerInput> getPlayers()
    {
        return players;
    }

    public List<PlayerInput> getAlivePlayers()
    {
        List<PlayerInput> alivePlayers = players
            .Where(p => p.GetComponent<FighterHealth>().GetCurrentLives() > 0)
            .ToList();
        return alivePlayers;
    }

}
