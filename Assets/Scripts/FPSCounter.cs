using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI text;

    float timeLeft = 0.5f;
    int frames = 0;

    void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = frames / 0.5f;
            text.text = Mathf.RoundToInt(fps).ToString();

            timeLeft = 0.5f;
            frames = 0;
        }
    }
}