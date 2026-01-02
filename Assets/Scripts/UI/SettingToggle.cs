using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingToggle : MonoBehaviour
{
    public string description;

    public TextMeshProUGUI descriptionText;
    public Toggle toggle;

    public void SetValue(bool value, string description = "")
    {
        if (description != "")
            this.description = description;

        descriptionText.text = this.description;
        toggle.isOn = value;
    }

}
