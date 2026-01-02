using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : MonoBehaviour
{
    public string description;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueText;
    public Slider slider;
    public int value;

    public void SetValue(int value, string description = "")
    {
        this.value = value;

        if (description != "")
            this.description = description;

        descriptionText.text = this.description;

        valueText.text = value.ToString();
        slider.value = value;
    }

    private void Update()
    {
        value = (int)slider.value;
        valueText.text = value.ToString();
    }

}
