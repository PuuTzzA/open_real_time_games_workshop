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
    [SerializeField]
    private GameObject defaultJoinScreen;

    [SerializeField] private GameObject playerSelectionPrefab;

    [Header("CharacterController Setup")] public Transform[] spawnPoints;
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
            spawnPoints = spawnPointsObject.GetComponentsInChildren<Transform>().ToArray();
            DontDestroyOnLoad(spawnPointsObject);
        }

        _pim.EnableJoining();

        if (players.Count > 0)
        {
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
                player.user.UnpairDevicesAndRemoveUser();
                Destroy(player.gameObject);
            }
            players.Clear();

                // Re-create selection panels
                foreach (var data in selectionData)
                {
                    var selection = PlayerInput.Instantiate(
                        playerSelectionPrefab,
                        playerIndex: data.Index,
                        controlScheme: data.Scheme,
                        pairWithDevices: data.Devices
                    );
                    var selMgr = selection.GetComponent<SelectionManager>();
                    selMgr.selectedCharacter = data.PrefabChoice;
                    selMgr.selectedColorIndex = data.ColorChoice;
                    DontDestroyOnLoad(selection.gameObject);
                    players.Add(selection);
                }
            foreach (var data in selectionData)
            {
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
        GameManager.PlayerChoices      = new List<int> { -1, -1, -1, -1 };
        GameManager.PlayerColorChoices = new List<int> { -1, -1, -1, -1 };


        foreach (var player in players)
        {
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
            character.GetComponent<BaseFighter>().playerColor = availableColors[data.ColorChoice];

            SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer.gameObject.GetComponent<UltHitbox>())
                {
                    renderer.material.SetFloat("_CanUlt", 1f);
                }
                renderer.material.SetColor("_Color", availableColors[data.ColorChoice]);
            }

            players.Add(character);
            DontDestroyOnLoad(character);
            character.transform.position = spawnPoints[i].position;
            foreach (var d in data.Devices)
                usedDevices.Add(d);
        }

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
