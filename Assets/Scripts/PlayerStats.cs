using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Configurable")]
    public float baseStamina = 100f;
    public float bonusMaxStamina = 125f;
    public float staminaDrainRate = 15f;
    public float staminaGainRate = 10f;
    public float bonusDelay = 5f;

    [Header("Stats")]
    public float currentStamina;
    public bool staminaBonus;
    public bool bonusActive;

    private Coroutine bonusCoroutine;

    public float TotalMaxStamina => bonusActive ? bonusMaxStamina : baseStamina;

    private void Start()
    {
        currentStamina = TotalMaxStamina;
    }

    public void DrainStamina()
    {
        // stop timer if running
        if (bonusCoroutine != null)
        {
            StopCoroutine(bonusCoroutine);
            bonusCoroutine = null;
        }

        staminaBonus = false;

        if (currentStamina <= 0f)
            return;

        currentStamina -= staminaDrainRate * Time.deltaTime;

        if (currentStamina <= baseStamina)
            bonusActive = false;

        currentStamina = Mathf.Clamp(currentStamina, 0f, TotalMaxStamina);
    }

    public void GainStamina()
    {
        // Regen up to base
        if (currentStamina < baseStamina)
        {
            currentStamina += staminaGainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, baseStamina);

            // If we are not full, timer should not run
            if (bonusCoroutine != null)
            {
                StopCoroutine(bonusCoroutine);
                bonusCoroutine = null;
            }

            return;
        }

        // We are full base stamina
        if (!bonusActive)
            currentStamina = TotalMaxStamina;

        // Start timer once while staying full
        if (!staminaBonus && bonusCoroutine == null)
            bonusCoroutine = StartCoroutine(BonusTimer());
    }

    private IEnumerator BonusTimer()
    {
        yield return new WaitForSeconds(bonusDelay);

        staminaBonus = true;     // unlocked
        bonusActive = true;      // capacity enabled
        currentStamina = bonusMaxStamina;

        bonusCoroutine = null;
    }
}
