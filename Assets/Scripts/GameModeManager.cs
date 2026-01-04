using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameMode
{
    Tag,
    Infection
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode gameMode;

    [Header("Tag Config")]
    public int tagTime = 10;
    public int tagLives = 10;

    [Header("Infection config")]
    public int rounds = 3;
    public int roundDuration = 3;
    public int laststandTime = 1;

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
    public bool lastman;
    public int nextInfected = 0;

    private Coroutine timerRoutine;
    private Coroutine scoreTickRoutine;
    public List<Player> players = new List<Player>();
    public List<Player> sortedPlayers = new List<Player>();
    private float endDelay = 9;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (GameSettings.Instance)
        {
            gameMode = GameSettings.Instance.gameMode;

            tagLives = GameSettings.Instance.tagLives;
            tagTime = GameSettings.Instance.tagTime;

            rounds = GameSettings.Instance.rounds;
            roundDuration = GameSettings.Instance.roundDuration;
            laststandTime = GameSettings.Instance.laststandTime;
        }
    }
    public void StartGame()
    {
        gameStarted = true;

        scoreTickRoutine = StartCoroutine(ScoreTick());

        StartTimer();

        // Give first player
        players[nextInfected].GainTagAbility(gameMode);
    }

    void Update()
    {
        if (!gameStarted)
            return;

        sortedPlayers = players.OrderByDescending(p => p.score).ToList();

        if (gameMode == GameMode.Tag)
        {
            foreach (Player player in players)
            {
                if (player.score == 0 || gameTimerFinished && !gameFinished)
                {
                    gameFinished = true;
                    CalculateGameWinner();
                }
            }
        }
        if (gameMode == GameMode.Infection)
        {
            if (IsLastManStanding() && !lastman)
            {
                lastman = true;
                playTimeLeft = minuteToSeconds(laststandTime);

            }
            else if (IsLastManStanding())
            {
                GetLastManStanding().lastman = true;
            }

            if (roundFinished && !calculateWinner)
            {
                finishedRounds++;

                foreach (var player in players)
                {
                    player.AddRoundScore();
                }

                if (finishedRounds == rounds)
                    CalculateGameWinner();
                else
                    CalculateRoundWinner();
            }

            // When timer has ended
            if (gameTimerFinished && !roundFinished)
            {
                roundFinished = true;

                if (finishedRounds == rounds)
                    gameFinished = true;

                foreach (var player in players)
                {
                    if (!player.tagAbility && !lastman)
                    {
                        player.AddRoundScore(30);
                    }
                }
            }
        }

        if (roundFinished)
            foreach (var player in players)
            {
                player.tagAbility = false;
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
        int maxScore = players.Max(p => p.roundScore);
        roundWinningPlayers = players.Where(p => p.roundScore == maxScore).Select(p => p.index).ToArray();

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
        playTimeLeft = minuteToSeconds(tagTime);

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

    public void ShowScoreBoard()
    {
        // Remove players to reveal score board screen
        foreach (var player in GameManager.Instance.players)
        {
            if (player != null)
                Destroy(player);
        }

        GameManager.Instance.players.Clear();
        GameManager.Instance.ScoreBoard.gameObject.SetActive(true);
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
        yield return new WaitForSeconds(3);

        ShowScoreBoard();

        yield return new WaitForSeconds(endDelay);

        SceneSwitcher.ReturnToMenu();
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3);

        ShowScoreBoard();

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
        lastman = false;

        nextInfected++;
        if (nextInfected == players.Count)
            nextInfected = 0;

        SceneSwitcher.LoadScene(GameSettings.Instance.map);
    }

    public int minuteToSeconds(int minute)
    {
        return minute * 60;
    }

    public IEnumerator LerpMaterialColor(Renderer renderer, Material fromMat, Material toMat, float duration)
    {
        Material mat = renderer.material;
        Color from = fromMat.color;
        Color to = toMat.color;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            mat.color = Color.Lerp(from, to, t);
            yield return null;
        }

        mat.color = to;
    }

    public void ChangeToColor(int index, bool tagger)
    {
        if (tagger)
            StartCoroutine(LerpMaterialColor(GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().Sweater,
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[index],
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[4],
                3f));
        else
            StartCoroutine(LerpMaterialColor(GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().Sweater,
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[4],
                GameManager.Instance.players[index].GetComponentInChildren<SplitScreenSetup>().materials[index],
                3f));
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

        if (GameSettings.Instance)
        {
            if (GameSettings.Instance.gameMode == GameMode.Tag)
            {
                score = GameModeManager.Instance.tagLives;
            }
        }
        else
            score = 1;
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

        GameModeManager.Instance.ChangeToColor(index, true);
    }

    public void LoseTagAbility(GameMode gameMode)
    {
        tagAbility = false;

        if (gameMode == GameMode.Tag)
            tagger = false;

        GameModeManager.Instance.ChangeToColor(index, false);
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
