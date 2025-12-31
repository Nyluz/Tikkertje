using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode
{
    Tag,
    Infection
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("MenuLabels")]
    public MenuLabel playerAmountLabel;
    public MenuLabel gameModeLabel;
    public MenuLabel mapLabel;

    [Header("PlayerButtons")]
    public List<Button> PlayerButtons;

    [Header("GameSettings")]
    public int playerAmount = 0;
    public GameMode gameMode;
    public string map;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetplayerAmount(int amount)
    {
        playerAmount = amount;
    }

    public void SetGameMode(int gameMode)
    {
        this.gameMode = (GameMode)gameMode;
    }

    public void SetMap(string map)
    {
        this.map = map;
    }

    public void StartGame()
    {
        if (playerAmount == 0)
            return;

        SceneManager.LoadSceneAsync(map);
    }

    private void Update()
    {
        playerAmountLabel.SetValue(playerAmount.ToString());
        gameModeLabel.SetValue(gameMode.ToString());
        mapLabel.SetValue(map);

        var gamepads = Gamepad.all;

        for (int i = 0; i < PlayerButtons.Count; i++)
        {
            if (gamepads.Count > i)
            {
                PlayerButtons[i].interactable = true;
                continue;
            }

            PlayerButtons[i].interactable = false;
        }

        if (gamepads.Count < playerAmount)
        {
            Debug.LogError("Not enough controllers connected");
            return;
        }
    }

}
