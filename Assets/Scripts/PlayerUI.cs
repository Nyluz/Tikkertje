using StarterAssets;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider staminaBar;
    public Image staminaFillImage;
    public Image staminaHandleImage;

    private PlayerStats stats;
    private FirstPersonController firstPersonController;
    private InputScript input;
    private PlayerInput playerInput;
    private EventSystem eventSystem;

    public Color staminaFillColor;
    public Color staminaFillColorBonus;
    public Color staminaHandleColor;
    public Color staminaHandleColorBonus;

    public Image crossHairImage;
    public Image bolt;
    public GameObject EscMenu;
    public GameObject firstButton;
    public GameObject winText;

    public TextMeshProUGUI timerText;
    public List<MenuLabel> playerScoreLabel;

    private void Awake()
    {
        stats = transform.parent.GetComponent<PlayerStats>();
        firstPersonController = transform.parent.GetComponent<FirstPersonController>();
        input = transform.parent.GetComponent<InputScript>();
        playerInput = transform.parent.GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Stamina bar
        SetStamina(stats.currentStamina);

        // Game timer
        if (GameModeManager.Instance.timeBased)
        {
            float t = GameModeManager.Instance.playTimeLeft;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }

        // Player scores
        for (int i = 0; i < GameModeManager.Instance.players.Count; i++)
        {
            playerScoreLabel[i].SetValue(GameModeManager.Instance.players[i].Score.ToString());
            playerScoreLabel[i].gameObject.SetActive(true);
        }

        // Crosshair only in FPS mode
        if (firstPersonController.mode() == FirstPersonController.Modes.thirdPerson)
            crossHairImage.enabled = false;
        else
            crossHairImage.enabled = true;

        // Esc menu
        if (input.escMenu)
            ToggleEscMenu();

        // Win text 
        winText.SetActive(GameModeManager.Instance.winningPlayers.Contains(playerInput.playerIndex));
    }

    public void SetCrosshair(Sprite sprite, int size)
    {
        crossHairImage.rectTransform.sizeDelta = new Vector2(size, size);
        crossHairImage.sprite = sprite;
    }

    public void SetStamina(float stamina)
    {
        staminaBar.value = stamina / stats.baseStamina;
        if (stats.bonusActive)
        {
            staminaFillImage.color = staminaFillColorBonus;
            staminaHandleImage.color = staminaHandleColorBonus;
            bolt.enabled = true;
        }
        else
        {
            staminaFillImage.color = staminaFillColor;
            staminaHandleImage.color = staminaHandleColor;
            bolt.enabled = false;
        }
    }

    public void ToggleEscMenu()
    {
        EscMenu.SetActive(!EscMenu.activeSelf);
        Time.timeScale = EscMenu.activeSelf ? 0f : 1f;
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("Menu");
    }

}
