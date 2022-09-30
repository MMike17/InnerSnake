using UnityEngine;

/// <summary>Class executing special commands to debug the game</summary>
public class DebugManager : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode loseKey;

#if DEBUG_MODE
    void Update()
    {
        if (Input.GetKeyDown(loseKey))
            FindObjectOfType<Player>().GameOver();
    }
#endif
}