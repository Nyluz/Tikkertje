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

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput.gameObject);
        playerInput.gameObject.GetComponentInChildren<SplitScreenCamera>().Setup();
        AssignController(Gamepad.all[playerInput.playerIndex], playerInput);
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

    private void Update()
    {
        gamepads = Gamepad.all;
        foreach (var gamepad in Gamepad.all)
        {
            Debug.Log(gamepad.device);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added)
            {
                Debug.Log("Gamepad connected: " + device.displayName);
            }
            else if (change == InputDeviceChange.Removed)
            {
                Debug.Log("Gamepad disconnected: " + device.displayName);
            }
        }
    }
}
