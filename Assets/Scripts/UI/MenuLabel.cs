using TMPro;
using UnityEngine;

public class MenuLabel : MonoBehaviour
{
    public string description;
    [HideInInspector] public string value;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueText;

    public void SetValue(string value, string description = "")
    {
        this.value = value;

        if (description != "")
            this.description = description;

        descriptionText.text = this.description;

        valueText.text = value;
    }
}
