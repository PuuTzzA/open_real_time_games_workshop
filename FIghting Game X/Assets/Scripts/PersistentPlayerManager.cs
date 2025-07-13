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

    [Header("UI References")] [SerializeField]
    private GameObject defaultJoinScreen;

    [SerializeField] private GameObject playerSelectionPrefab;

    [Header("CharacterController Setup")] public Transform[] spawnPoints;
    [SerializeField] private GameObject spawnPointsObject;
    public GameObject[] characterPrefabs;

    // For testing
    private string fightScene = "TestSceneMartin";

    private void Awake()
    {

        Debug.Log("PersistentPlayerManager Awake");
        Debug.Log($"{InputUser.all.Count} InputUsers before initialization");
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Debug.Log("PersistentPlayerManager Instance created");

        if (spawnPointsObject == null)
            spawnPointsObject = GameObject.Find("SpawnPointsMapping");

        _pim = GetComponent<PlayerInputManager>();
        players = new List<PlayerInput>();
        _pim.EnableJoining();

        // Clear any leftover PlayerInputs
        foreach (var player in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
        {
            player.user.UnpairDevicesAndRemoveUser();
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
        Debug.Log($"Player joined: {player.playerIndex}, scheme: {player.currentControlScheme}");

        // Avoid duplicates
        if (players.Any(p => p.playerIndex == player.playerIndex))
            return;

        DontDestroyOnLoad(player.gameObject);
        players.Add(player);

        if (defaultJoinScreen != null)
            defaultJoinScreen.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    Debug.Log($"Scene loaded: {scene.name}");

    if (scene.name == fightScene)
    {
        Debug.Log("Entering fight scene. Disabling joining.");
        _pim.DisableJoining();
        StartCoroutine(SpawnAllPlayers());
    }
    else if (scene.name == "CharacterSelection")
    {
        Debug.Log("Entering CharacterSelection scene.");

        if (spawnPointsObject == null)
        {
            spawnPointsObject = GameObject.Find("SpawnPointsMapping");
            spawnPoints = spawnPointsObject.GetComponentsInChildren<Transform>().ToArray();
            DontDestroyOnLoad(spawnPointsObject);
            Debug.Log("SpawnPointsMapping reloaded.");
        }

        _pim.EnableJoining();

        if (players.Count > 0)
        {
            Debug.Log($"Reinitializing {players.Count} player selection panels...");
            var selectionData = players.Select(p => new
            {
                PrefabChoice = GameManager.PlayerChoices[p.playerIndex],
                ColorChoice  = GameManager.PlayerColorChoices[p.playerIndex],
                Index        = p.playerIndex,
                Scheme       = p.currentControlScheme,
                Devices      = p.devices.ToArray()
            }).ToList();

            foreach (var player in players)
            {
                Debug.Log($"Tearing down player: index={player.playerIndex}");
                player.user.UnpairDevicesAndRemoveUser();
                Destroy(player.gameObject);
            }
            players.Clear();

            foreach (var data in selectionData)
            {
                Debug.Log($"Instantiating selection panel: index={data.Index}, prefab={data.PrefabChoice}, color={data.ColorChoice}");
                var selection = PlayerInput.Instantiate(
                    playerSelectionPrefab,
                    playerIndex: data.Index,
                    controlScheme: data.Scheme,
                    pairWithDevices: data.Devices
                );

                var selMgr = selection.GetComponent<SelectionManager>();
                selMgr.selectedCharacter  = data.PrefabChoice;
                selMgr.selectedColorIndex = data.ColorChoice;

                DontDestroyOnLoad(selection.gameObject);
                players.Add(selection);
            }

            if (defaultJoinScreen != null)
                defaultJoinScreen.SetActive(false);
        }
    }
    else if (scene.name == "MainMenu")
    {
        Debug.Log("Entering MainMenu. Resetting player state.");
        GameManager.PlayerChoices      = new List<int> { -1, -1, -1, -1 };
        GameManager.PlayerColorChoices = new List<int> { -1, -1, -1, -1 };

        Debug.Log($"InputUser count before cleanup: {InputUser.all.Count}");

        foreach (var player in players)
        {
            Debug.Log($"Destroying player {player.playerIndex}");
            player.user.UnpairDevicesAndRemoveUser();
            Destroy(player.gameObject);
        }
        players.Clear();

        foreach (var user in InputUser.all.ToArray())
        {
            Debug.Log($"Cleaning up orphaned InputUser {user.id}");
            user.UnpairDevicesAndRemoveUser();
        }

        Debug.Log($"InputUser count after cleanup: {InputUser.all.Count}");

        _instance = null;
        Destroy(spawnPointsObject);
        Destroy(gameObject);
    }
}

private IEnumerator SpawnAllPlayers()
{
    Debug.Log("Starting SpawnAllPlayers...");
    yield return null;

    var playerData = players.Select(p => new
    {
        Index        = p.playerIndex,
        PrefabChoice = GameManager.PlayerChoices[p.playerIndex],
        ColorChoice  = GameManager.PlayerColorChoices[p.playerIndex],
        Scheme       = p.currentControlScheme,
        Devices      = p.devices.ToArray()
    }).ToList();

    foreach (var player in players)
    {
        Debug.Log($"Clearing old player: {player.playerIndex}");
        if (player.user.valid) player.user.UnpairDevicesAndRemoveUser();
        Destroy(player.gameObject);
    }
    players.Clear();

    var usedDevices = new HashSet<InputDevice>();
    for (int i = 0; i < playerData.Count; i++)
    {
        var data = playerData[i];

        if (data.Devices.Any(d => usedDevices.Contains(d)))
        {
            Debug.LogWarning($"Skipping player {data.Index} – duplicate device detected.");
            continue;
        }

        if (data.PrefabChoice < 0 || data.PrefabChoice >= characterPrefabs.Length)
        {
            Debug.LogWarning($"Invalid prefab choice for player {data.Index}. Skipping.");
            continue;
        }

        Debug.Log($"Spawning fighter {data.Index} using prefab {data.PrefabChoice}");

        var character = PlayerInput.Instantiate(
            characterPrefabs[data.PrefabChoice],
            playerIndex: data.Index,
            controlScheme: data.Scheme,
            pairWithDevices: data.Devices
        );

        character.GetComponent<OffscreenMarker>().Color = availableColors[data.ColorChoice];
        SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.gameObject.GetComponent<UltHitbox>())
            {
                renderer.material.SetFloat("_CanUlt", 1f);
            }
            renderer.material.SetColor("_Color", availableColors[data.ColorChoice]);
        }

        var fighter = character.GetComponent<BaseFighter>();
        fighter.playerColor = availableColors[data.ColorChoice];

        players.Add(character);
        DontDestroyOnLoad(character);
        character.transform.position = spawnPoints[i].position;

        foreach (var d in data.Devices)
            usedDevices.Add(d);
    }

    Debug.Log($"Spawned {players.Count} fighters.");

    IngameUI ui = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
    while (ui == null)
    {
        Debug.LogWarning("Waiting for IngameUI...");
        ui = FindAnyObjectByType<IngameUI>(FindObjectsInactive.Include);
        yield return null;
    }

    ui.gameObject.SetActive(true);
    Debug.Log("IngameUI activated.");
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
