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

    public GameObject blackScreen;
    public GameObject splitscreenSelect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += HandlePlayerJoined;
    }

    private void Start()
    {
        if (GameSettings.Instance)
        {
            StartSplitscreen(GameSettings.Instance.playerAmount);
        }
    }

    public void StartSplitscreen(int playerCount)
    {
        var gamepads = Gamepad.all;

        if (gamepads.Count < playerCount)
        {
            Debug.LogError("Not enough controllers connected");
            return;
        }

        blackScreen.SetActive(true);
        splitscreenSelect.SetActive(false);

        // Player 1
        if (playerCount >= 1)
        {
            PlayerInputManager.instance.JoinPlayer(
                playerIndex: 0,
                splitScreenIndex: 0,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[0]
            );
            Player player = new Player(0);
            GameModeManager.Instance.players.Add(player);
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
            Player player = new Player(1);
            GameModeManager.Instance.players.Add(player);
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
            Player player = new Player(2);
            GameModeManager.Instance.players.Add(player);
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
            Player player = new Player(3);
            GameModeManager.Instance.players.Add(player);
        }

        GameModeManager.Instance.StartGame();
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
            player.GetComponentInChildren<SplitScreenSetup>().Setup();

        if (players.Count == 3 && mainCamera != null)
        {
            var mainCam = mainCamera;
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

    private void OnDestroy()
    {
        Destroy(GameSettings.Instance);
        GameSettings.Instance = null;
    }

}
