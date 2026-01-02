using StarterAssets;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    public Color staminaFillColor;
    public Color staminaFillColorBonus;
    public Color staminaHandleColor;
    public Color staminaHandleColorBonus;

    public Image crossHairImage;
    public Image boltImage;
    public Image infectedImage;
    public Image lastmanImage;
    public Image taggerImage;
    public GameObject EscMenu;
    public GameObject firstButton;
    public GameObject gameWinText;
    public GameObject roundWinText;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreUnitText;

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
        if (GameModeManager.Instance.roundFinished || GameModeManager.Instance.gameFinished)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            float t = GameModeManager.Instance.playTimeLeft;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
        }

        // Score text
        if (GameModeManager.Instance.gameMode == GameMode.Tag)
        {
            scoreText.text = GameModeManager.Instance.players[playerInput.playerIndex].score.ToString();
            scoreUnitText.text = "Lives";
        }

        if (GameModeManager.Instance.gameMode == GameMode.Infection)
        {
            scoreText.text = GameModeManager.Instance.players[playerInput.playerIndex].roundScore.ToString();
            scoreUnitText.text = "Score";
        }

        // Crosshair only in FPS mode
        if (firstPersonController.mode() == FirstPersonController.Modes.thirdPerson)
            crossHairImage.enabled = false;
        else
            crossHairImage.enabled = true;

        // Show tagger icon
        taggerImage.gameObject.SetActive(GameModeManager.Instance.players[playerInput.playerIndex].tagger);

        // Show infected icon
        infectedImage.gameObject.SetActive(GameModeManager.Instance.players[playerInput.playerIndex].infected);

        // Show lastman icon
        lastmanImage.gameObject.SetActive(GameModeManager.Instance.players[playerInput.playerIndex].lastman);

        // Esc menu
        if (input.escMenu)
            ToggleEscMenu();

        // Round win text
        roundWinText.SetActive(GameModeManager.Instance.roundWinningPlayers.Contains(playerInput.playerIndex));

        // Game win text 
        gameWinText.SetActive(GameModeManager.Instance.gameWinningPlayers.Contains(playerInput.playerIndex));
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
            boltImage.enabled = true;
        }
        else
        {
            staminaFillImage.color = staminaFillColor;
            staminaHandleImage.color = staminaHandleColor;
            boltImage.enabled = false;
        }
    }

    public void ToggleEscMenu()
    {
        bool opening = !EscMenu.activeSelf;

        EscMenu.SetActive(opening);
        Time.timeScale = opening ? 0f : 1f;

        EventSystem.current.SetSelectedGameObject(firstButton);

        if (!opening)
            StartCoroutine(UnblockJumpNextFrame());
    }

    IEnumerator UnblockJumpNextFrame()
    {
        firstPersonController.blockJumpInput = true;
        yield return null;
        firstPersonController.blockJumpInput = false;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneSwitcher.ReturnToMenu();
    }

}
