using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    PlayerInputManager playerInputManager;
    public List<GameObject> players;
    public ReadOnlyArray<Gamepad> gamepads;

    public List<Transform> spawnPoints;
    public Camera mainCamera;

    public bool hasKeyboard = true;
    public bool splitScreenStarted;

    public GameObject blackScreen;
    public GameObject splitscreenSelect;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += HandlePlayerJoined;
    }

    public void StartSplitscreen(int playerCount)
    {
        blackScreen.SetActive(true);
        splitscreenSelect.SetActive(false);

        var gamepads = Gamepad.all;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (gamepads.Count < playerCount)
        {
            Debug.LogError("Not enough controllers connected");
            return;
        }

        // Player 1
        if (playerCount >= 1)
        {
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 0,
                splitScreenIndex: 0,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[0]
            );
        }
        // Player 2
        if (playerCount >= 2)
        {
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 1,
                splitScreenIndex: 1,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[1]
            );
        }
        // Player 3
        if (playerCount >= 3)
        {
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 2,
                splitScreenIndex: 2,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[2]
            );
        }
        // Player 4
        if (playerCount == 4)
        {
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 3,
                splitScreenIndex: 3,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[3]
            );
        }

        splitScreenStarted = true;
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        GameObject playerObject = playerInput.gameObject;
        int playerIndex = playerInput.playerIndex;
        CharacterController characterController = playerObject.GetComponent<CharacterController>();

        // Add new player to player list
        players.Add(playerObject);

        // Assign controller to player
        if (playerInput.currentControlScheme == "Gamepad")
            AssignController(Gamepad.all[playerIndex], playerInput);
        else
            print("keybaord end mous");

        // Give player spawn position and rotation
        if (playerIndex < spawnPoints.Count)
        {
            Spawn(playerInput, characterController);
        }

        // Recalculate the splitscreens
        foreach (var player in players)
            player.GetComponentInChildren<SplitScreenCamera>().Setup();

        if (players.Count == 3 && GameManager.Instance.mainCamera != null)
        {
            var mainCam = GameManager.Instance.mainCamera;
            mainCam.enabled = true;
            mainCam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
        }
    }

    public void Spawn(PlayerInput playerInput, CharacterController characterController)
    {
        characterController.enabled = false;
        playerInput.transform.position = spawnPoints[playerInput.playerIndex].localPosition;
        playerInput.transform.rotation = spawnPoints[playerInput.playerIndex].localRotation;
        characterController.enabled = true;
    }

    public void AssignController(Gamepad gamepad, PlayerInput playerInput)
    {
        // Unpair existing devices
        playerInput.user.UnpairDevices();

        // Pair this controller
        InputUser.PerformPairingWithDevice(gamepad, playerInput.user);
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
