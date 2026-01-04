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

    [Header("Tag Sliders")]
    public Transform tagSettingsPanel;
    public SettingSlider tagGameTime;
    public SettingSlider tagLives;

    [Header("Infection Sliders")]
    public Transform infectionSettingsPanel;
    public SettingSlider roundsSlider;
    public SettingSlider roundDurationSlider;
    public SettingSlider lastmanDurationSlider;

    [Header("Bomb Sliders")]
    public Transform bombSettingsPanel;
    public SettingSlider bombLivesSlider;
    public SettingSlider bombFuseTimeSlider;
    public SettingSlider bombRoundsSlider;

    public Button infectionButton;

    [Header("PlayerButtons")]
    public List<Button> PlayerButtons;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedGameObject);

        // Tag
        tagGameTime.SetValue(10);
        tagLives.SetValue(10);

        // Infection
        roundsSlider.SetValue(8);
        roundDurationSlider.SetValue(3);
        lastmanDurationSlider.SetValue(1);

        // Bomb
        bombFuseTimeSlider.SetValue(1);
        bombLivesSlider.SetValue(5);
        bombRoundsSlider.SetValue(5);
    }

    private void Update()
    {
        playerAmountLabel.SetValue(GameSettings.Instance.playerAmount.ToString());
        gameModeLabel.SetValue(GameSettings.Instance.gameMode.ToString());
        mapLabel.SetValue(GameSettings.Instance.map);

        SetTagGameSettings();
        SetInfectionGameSettings();
        SetBombGameSettings();

        SetGameModeSettings();

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

    public void SetTagGameSettings()
    {
        GameSettings.Instance.tagTime = tagGameTime.value;
        GameSettings.Instance.tagLives = tagLives.value;
    }

    public void SetInfectionGameSettings()
    {
        GameSettings.Instance.rounds = roundsSlider.value;
        GameSettings.Instance.roundDuration = roundDurationSlider.value;
        GameSettings.Instance.laststandTime = lastmanDurationSlider.value;
    }

    public void SetBombGameSettings()
    {
        GameSettings.Instance.bombLives = bombLivesSlider.value;
        GameSettings.Instance.fuseTime = bombFuseTimeSlider.value;
        GameSettings.Instance.bombRounds = bombRoundsSlider.value;
    }

    public void SetGameModeSettings()
    {
        tagSettingsPanel.gameObject.SetActive(GameSettings.Instance.gameMode == GameMode.Tag);
        infectionSettingsPanel.gameObject.SetActive(GameSettings.Instance.gameMode == GameMode.Infection);
        bombSettingsPanel.gameObject.SetActive(GameSettings.Instance.gameMode == GameMode.Bomb);

        if (GameSettings.Instance.gameMode == GameMode.Tag)
        {
            var nav = infectionButton.navigation;
            nav.selectOnDown = tagGameTime.slider;
            infectionButton.navigation = nav;
        }

        if (GameSettings.Instance.gameMode == GameMode.Infection)
        {
            var nav = infectionButton.navigation;
            nav.selectOnDown = roundsSlider.slider;
            infectionButton.navigation = nav;
        }
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
