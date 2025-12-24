using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public class GameManager : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public List<GameObject> players;
    public ReadOnlyArray<Gamepad> gamepads;

    public List<Transform> spawnPoints;
    [SerializeField] private Camera mainCamera;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += HandlePlayerJoined;
    }

    private void Start()
    {
        mainCamera.enabled = false;
    }

    public void StartSplitscreen(int playerCount)
    {
        var gamepads = Gamepad.all;

        if (gamepads.Count < playerCount)
        {
            Debug.LogError("Not enough controllers connected");
            return;
        }
        if (playerCount >= 1)
        {
            // Player 1
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 0,
                splitScreenIndex: 0,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[0]
            );
        }
        if (playerCount >= 2)
        {
            // Player 2
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 1,
                splitScreenIndex: 1,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[1]
            );
        }
        if (playerCount >= 3)
        {
            // Player 3
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 2,
                splitScreenIndex: 2,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[2]
            );
        }
        if (playerCount == 4)
        {
            // Player 4
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 3,
                splitScreenIndex: 3,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[3]
            );
        }
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        GameObject playerObject = playerInput.gameObject;
        int playerIndex = playerInput.playerIndex;
        CharacterController playerController = playerObject.GetComponent<CharacterController>();

        // Add new player to player list
        players.Add(playerObject);

        // Assign controller to player
        AssignController(Gamepad.all[playerIndex], playerInput);

        // Give player spawn position and rotation
        if (playerIndex < spawnPoints.Count)
        {
            playerController.enabled = false;
            playerInput.transform.position = spawnPoints[playerIndex].localPosition;
            playerInput.transform.rotation = spawnPoints[playerIndex].localRotation;
            playerController.enabled = true;
        }

        // Recalculate the splitscreens
        foreach (var player in players)
            player.GetComponentInChildren<SplitScreenCamera>().Setup();
    }

    public void AssignController(Gamepad gamepad, PlayerInput playerInput)
    {
        // Unpair existing devices
        playerInput.user.UnpairDevices();

        // Pair this controller
        InputUser.PerformPairingWithDevice(gamepad, playerInput.user);

        // Activate correct control scheme
        playerInput.SwitchCurrentControlScheme(gamepad);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added)
            {
                //Debug.Log("Gamepad connected: " + device.displayName);
            }
            else if (change == InputDeviceChange.Removed)
            {
                //Debug.Log("Gamepad disconnected: " + device.displayName);
            }
        }
    }
}
