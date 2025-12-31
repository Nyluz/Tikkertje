using TMPro;
using UnityEngine;

public class MenuLabel : MonoBehaviour
{
    public string description;
    [HideInInspector] public string value;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueText;

    public void SetValue(string value)
    {
        this.value = value;

        descriptionText.text = description;
        valueText.text = value;

    }
}
