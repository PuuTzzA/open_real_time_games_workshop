using System;
using UnityEngine;
using TMPro;

public class LoadCharacter : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;
    public TMP_Text label;

    private void Start()
    {
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        GameObject prefab = characterPrefabs[selectedCharacter];
        GameObject character = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        label.text = prefab.name;
    }
}