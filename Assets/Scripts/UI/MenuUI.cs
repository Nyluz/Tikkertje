using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("MenuLabels")]
    public MenuLabel playerAmountLabel;
    public MenuLabel gameModeLabel;
    public MenuLabel mapLabel;
    public GameObject firstSelectedGameObject;

    [Header("PlayerButtons")]
    public List<Button> PlayerButtons;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedGameObject);
    }

    private void Update()
    {
        playerAmountLabel.SetValue(GameSettings.Instance.playerAmount.ToString());
        gameModeLabel.SetValue(GameSettings.Instance.gameMode.ToString());
        mapLabel.SetValue(GameSettings.Instance.map);

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

        if (gamepads.Count < GameSettings.Instance.playerAmount)
        {
            Debug.LogError("Not enough controllers connected");
            return;
        }
    }
    public void SetplayerAmount(int amount)
    {
        GameSettings.Instance.playerAmount = amount;
    }

    public void SetGameMode(int gameMode)
    {
        GameSettings.Instance.gameMode = (GameMode)gameMode;
    }

    public void SetMap(string map)
    {
        GameSettings.Instance.map = map;
    }

    public void StartGame()
    {
        if (GameSettings.Instance.playerAmount == 0)
            return;

        SceneManager.LoadSceneAsync(GameSettings.Instance.map);
    }

    public void SetSelectedGameObject(GameObject gameObject)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
