using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider staminaBar;
    public Image staminaFillImage;
    public Image staminaHandleImage;

    private PlayerStats stats;
    private FirstPersonController firstPersonController;
    private InputScript input;
    private EventSystem eventSystem;

    public Color staminaFillColor;
    public Color staminaFillColorBonus;
    public Color staminaHandleColor;
    public Color staminaHandleColorBonus;

    public Image crossHairImage;
    public Image bolt;
    public GameObject EscMenu;
    public GameObject firstButton;

    private void Awake()
    {
        stats = transform.parent.GetComponent<PlayerStats>();
        firstPersonController = transform.parent.GetComponent<FirstPersonController>();
        input = transform.parent.GetComponent<InputScript>();
    }

    void Update()
    {
        // Stamina bar
        SetStamina(stats.currentStamina);

        // Crosshair only in FPS mode
        if (firstPersonController.mode() == FirstPersonController.Modes.thirdPerson)
            crossHairImage.enabled = false;
        else
            crossHairImage.enabled = true;

        // Esc menu
        if (input.escMenu)
            ToggleEscMenu();

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

}
