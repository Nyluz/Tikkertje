using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider staminaBar;
    public Image staminaFillImage;
    public Image staminaHandleImage;

    private PlayerStats stats;
    private FirstPersonController firstPersonController;

    public Color staminaFillColor;
    public Color staminaFillColorBonus;
    public Color staminaHandleColor;
    public Color staminaHandleColorBonus;

    public Image crossHairImage;
    public Image bolt;

    private void Awake()
    {
        stats = transform.parent.GetComponent<PlayerStats>();
        firstPersonController = transform.parent.GetComponent<FirstPersonController>();
    }

    void Update()
    {
        SetStamina(stats.currentStamina);
        if (firstPersonController.mode() == FirstPersonController.Modes.thirdPerson)
            crossHairImage.enabled = false;
        else
            crossHairImage.enabled = true;
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
}
