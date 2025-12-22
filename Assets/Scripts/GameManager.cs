using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public List<GameObject> players;

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput.gameObject);
        playerInput.gameObject.GetComponentInChildren<SplitScreenCamera>().Setup();
    }
}
