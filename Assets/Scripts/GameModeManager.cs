using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode gameMode;

    [Header("General Config")]
    public bool timeBased = false;
    public int playTime = 1;
    public int lastStandTime = 1;
    public int playPoints = 5;

    [Header("Infection config")]
    public int maxRounds = 3;

    [Header("General State")]
    public float playTimeLeft;
    public bool gameStarted;
    public bool gameTimerFinished;
    public bool gameFinished;
    public bool calculateWinner;
    public int[] gameWinningPlayers;
    public int[] roundWinningPlayers;

    [Header("Infection State")]
    public bool roundFinished;
    public int finishedRounds;
    public bool lastStand;
    public int nextInfected = 0;

    private Coroutine timerRoutine;
    private Coroutine scoreTickRoutine;
    public List<Player> players = new List<Player>();
    public List<Player> sortedPlayers = new List<Player>();
    private float endDelay = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (GameSettings.Instance)
        {
            gameMode = GameSettings.Instance.gameMode;
        }
    }
    public void StartGame()
    {
        gameStarted = true;

        scoreTickRoutine = StartCoroutine(ScoreTick());

        if (timeBased)
            StartTimer();

        // Random player has Tag ability
        players[nextInfected].GainTagAbility(gameMode);
    }

    void Update()
    {
        if (!gameStarted)
            return;

        sortedPlayers = players.OrderByDescending(p => p.score).ToList();

        if (gameMode == GameMode.Tag)
        {
            // Score based logic
            if (!timeBased)
            {
                foreach (Player player in players)
                {
                    if (player.score == 0 && !gameFinished)
                    {
                        gameFinished = true;
                        CalculateGameWinner();
                    }
                }
            }
            // Time based logic
            else
            {
                if (gameTimerFinished && !gameFinished)
                {
                    gameFinished = true;
                    CalculateGameWinner();
                }
            }
        }

        if (gameMode == GameMode.Infection)
        {
            timeBased = true;

            if (IsLastManStanding() && !lastStand)
            {
                lastStand = true;
                playTimeLeft = minuteToSeconds(lastStandTime);

            }
            else if (IsLastManStanding())
            {
                GetLastManStanding().lastman = true;
            }

            if (roundFinished && !calculateWinner)
            {
                foreach (var player in players)
                {
                    player.AddRoundScore();
                }

                if (finishedRounds == maxRounds)
                    CalculateGameWinner();
                else
                    CalculateRoundWinner();
            }

            // When timer has ended
            if (gameTimerFinished && !roundFinished)
            {
                roundFinished = true;

                if (gameMode == GameMode.Infection)
                {
                    roundFinished = true;

                    if (finishedRounds == maxRounds)
                        gameFinished = true;
                }

                foreach (var player in players)
                {
                    if (!player.tagAbility && !lastStand)
                    {
                        player.AddRoundScore(30);
                    }
                    else if (!player.tagAbility && lastStand)
                    {
                        player.AddRoundScore(60);
                    }
                }
            }
        }
    }

    public void CalculateGameWinner()
    {
        StopTimer();
        calculateWinner = true;
        int maxScore = players.Max(p => p.score);
        gameWinningPlayers = players.Where(p => p.score == maxScore).Select(p => p.index).ToArray();

        StartCoroutine(ReturnToMenu());
    }

    public void CalculateRoundWinner()
    {
        StopTimer();
        calculateWinner = true;
        int maxScore = players.Max(p => p.score);
        roundWinningPlayers = players.Where(p => p.score == maxScore).Select(p => p.index).ToArray();

        StartCoroutine(ReloadScene());
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

        StopCoroutine(scoreTickRoutine);
    }

    private IEnumerator Timer()
    {
        playTimeLeft = minuteToSeconds(playTime);

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
            players[playerIndex].AddRoundScore(5);

            if (players[targetPlayerIndex].lastman)
            {
                roundFinished = true;
                return;
            }

            players[targetPlayerIndex].GainTagAbility(gameMode);
        }
    }

    IEnumerator ScoreTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (IsLastManStanding())
                GetLastManStanding().AddRoundScore(1);
        }
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(endDelay);
        SceneSwitcher.ReturnToMenu();
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(endDelay);

        // Reset player stats
        foreach (var player in players)
        {
            player.ResetStats();
        }

        // Reset bools
        roundWinningPlayers = new int[0];
        roundFinished = false;
        gameTimerFinished = false;
        calculateWinner = false;

        finishedRounds++;
        nextInfected++;
        if (nextInfected == players.Count)
            nextInfected = 0;

        SceneSwitcher.LoadScene(GameSettings.Instance.map);
    }

    public int minuteToSeconds(int minute)
    {
        return minute * 60;
    }

    public void ChangeColor(int index, bool tagger)
    {
        if (tagger)
        {
            GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().Sweater.material =
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[4];
        }
        else
        {
            GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().Sweater.material =
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[index];
        }
    }
}

[Serializable]
public class Player
{
    public int index;
    public int score;
    public int roundScore;
    public bool tagAbility;
    public bool lastman;
    public bool infected;
    public bool tagger;

    public Player(int index)
    {
        this.index = index;
        score = 5;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddRoundScore(int amount)
    {
        roundScore += amount;
    }

    public void ReduceScore(int amount)
    {
        score -= amount;
    }

    public void GainTagAbility(GameMode gameMode)
    {
        tagAbility = true;

        if (gameMode == GameMode.Tag)
            tagger = true;

        if (gameMode == GameMode.Infection)
            infected = true;

        GameModeManager.Instance.ChangeColor(index, true);
    }

    public void LoseTagAbility(GameMode gameMode)
    {
        tagAbility = false;

        if (gameMode == GameMode.Tag)
            tagger = false;

        GameModeManager.Instance.ChangeColor(index, false);
    }

    public void ResetStats()
    {
        tagAbility = false;
        infected = false;
        tagger = false;
        lastman = false;
        roundScore = 0;
    }

    public void AddRoundScore()
    {
        score += roundScore;
    }

}
