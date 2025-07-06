using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    // List of each character prefab chosen by the players. Player 1 choice in index 0, etc...
    public static List<int> PlayerChoices = new List<int> { -1, -1, -1, -1 };
    public static List<int> PlayerColorChoices = new List<int> { -1, -1, -1, -1 };
}