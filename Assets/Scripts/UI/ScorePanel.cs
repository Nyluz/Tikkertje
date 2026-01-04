using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePanel : MonoBehaviour
{
    public List<MenuLabel> playerScoreLabel;
    public TextMeshProUGUI roundText;

    void Update()
    {
        // Player scores
        for (int i = 0; i < GameModeManager.Instance.sortedPlayers.Count; i++)
        {
            var player = GameModeManager.Instance.sortedPlayers[i];

            playerScoreLabel[i].SetValue(player.score.ToString(), $"Player {player.index + 1}");
            playerScoreLabel[i].gameObject.SetActive(true);
        }

        roundText.text = "Round: " + GameModeManager.Instance.finishedRounds + "/" + GameModeManager.Instance.rounds;
    }
}
