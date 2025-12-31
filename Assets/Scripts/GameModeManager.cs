using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode gameMode;

    [Header("General Config")]
    public bool timeBased = false;
    public int playTime = 2;
    public int playPoints = 5;

    [Header("Infection config")]
    public int maxRounds;

    [Header("General State")]
    public bool gameStarted;
    public float playTimeLeft;
    public bool gameTimerFinished;
    public bool gameFinished;
    public int[] winningPlayers;

    [Header("Infection State")]
    public bool roundFinished;
    public int finishedRounds;
    public bool lastStand;

    private Coroutine timerRoutine;
    public List<Player> players = new List<Player>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GameSettings.Instance)
        {
            gameMode = GameSettings.Instance.gameMode;
        }
    }

    void Update()
    {
        if (!gameStarted)
            return;

        if (gameMode == GameMode.Tag)
        {
            // Score based logic
            if (!timeBased)
            {
                foreach (Player player in players)
                {
                    if (player.Score == 0 && !gameFinished)
                    {
                        gameFinished = true;
                        CalculateWinner();
                    }
                }
            }
            // Time based logic
            else
            {
                if (gameTimerFinished && !gameFinished)
                {
                    gameFinished = true;
                    CalculateWinner();
                }
            }
        }

        if (gameMode == GameMode.Infection)
        {
            timeBased = true;

            if (IsLastManStanding())
            {
                lastStand = true;
                GetLastManStanding().lastman = true;
            }

            if (roundFinished)
            {
                CalculateWinner();
            }

            // When timer has ended
            if (gameTimerFinished && !roundFinished)
            {
                roundFinished = true;

                foreach (var player in players)
                {
                    if (!player.tagAbility && !lastStand)
                    {
                        player.AddScore(30);
                    }
                    else if (!player.tagAbility && lastStand)
                    {
                        player.AddScore(60);
                    }
                }
            }
        }
    }

    private bool IsLastManStanding()
    {
        return players.Count(p => !p.tagAbility) == 1;
    }

    private Player GetLastManStanding()
    {
        var remaining = players.Where(p => !p.tagAbility).ToList();
        return remaining.Count == 1 ? remaining[0] : null;
    }

    public void StartGame()
    {
        gameStarted = true;

        if (timeBased)
            StartTimer();

        // Random player has Tag ability
        players[Random.Range(0, players.Count)].GainTagAbility(gameMode);
    }

    public void CalculateWinner()
    {
        int maxScore = players.Max(p => p.Score);
        winningPlayers = players.Where(p => p.Score == maxScore).Select(p => p.Index).ToArray();

        StartCoroutine(ReturnToMenu());
    }

    public void StartTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(Timer());
    }

    public void StopTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
    }

    private IEnumerator Timer()
    {
        playTimeLeft = playTime * 60;

        while (playTimeLeft > 0f)
        {
            playTimeLeft -= Time.deltaTime;
            yield return null;
        }

        OnTimerFinished();
    }

    private void OnTimerFinished()
    {
        StopTimer();
        gameTimerFinished = true;
        playTimeLeft = 0f;

        if (gameMode == GameMode.Tag)
        {
            gameFinished = true;
        }

        if (gameMode == GameMode.Infection)
        {
            roundFinished = true;
            finishedRounds++;

            if (finishedRounds == maxRounds)
                gameFinished = true;
        }
    }

    public void SlapAction(int playerIndex, int targetPlayerIndex)
    {
        if (gameMode == GameMode.Tag)
        {
            players[targetPlayerIndex].ReduceScore(1);
            players[targetPlayerIndex].GainTagAbility(gameMode);
            players[playerIndex].LoseTagAbility(gameMode);
            players[targetPlayerIndex].tagger = true;
        }
        if (gameMode == GameMode.Infection)
        {
            players[playerIndex].AddScore(1);

            if (players[targetPlayerIndex].lastman)
            {
                roundFinished = true;
                return;
            }

            players[targetPlayerIndex].GainTagAbility(gameMode);
        }
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(7f);
        SceneManager.LoadScene("Menu");
    }
}

public class Player
{
    public int Index { get; private set; }
    public int Score { get; private set; }
    public bool tagAbility { get; private set; }

    public bool lastman;
    public bool infected;
    public bool tagger;

    public Player(int index)
    {
        Index = index;
        Score = 5;
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void ReduceScore(int amount)
    {
        Score -= amount;
    }

    public void GainTagAbility(GameMode gameMode)
    {
        tagAbility = true;

        if (gameMode == GameMode.Tag)
            tagger = true;

        if (gameMode == GameMode.Infection)
            infected = true;

    }

    public void LoseTagAbility(GameMode gameMode)
    {
        tagAbility = false;

        if (gameMode == GameMode.Tag)
            tagger = false;
    }

}
