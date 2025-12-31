using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode gameMode;

    [Header("Config")]
    public bool timeBased = false;
    public int playTime = 2;
    public int playPoints = 5;

    [Header("State")]
    public bool gameStarted;
    public float playTimeLeft;
    public bool gameTimerFinished;
    public bool gameFinished;
    public int[] winningPlayers;

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
    }

    void Start()
    {
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
                        StartCoroutine(ReturnToMenu());
                    }
                }
            }
            else
            {
                if (gameTimerFinished && !gameFinished)
                {
                    gameFinished = true;
                    CalculateWinner();
                    StartCoroutine(ReturnToMenu());
                }
            }
        }

        if (gameMode == GameMode.Infection)
        {

        }
    }

    public void StartGame()
    {
        gameStarted = true;

        if (timeBased)
            StartTimer();

        // Random player has Tag ability
        players[Random.Range(0, players.Count)].GainTagAbility();
    }

    public void CalculateWinner()
    {
        int maxScore = players.Max(p => p.Score);
        winningPlayers = players.Where(p => p.Score == maxScore).Select(p => p.Index).ToArray();
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
        Debug.Log("Game mode timer finished");
    }

    public void SlapAction(int playerIndex, int targetPlayerIndex)
    {
        if (gameMode == GameMode.Tag)
        {
            players[targetPlayerIndex].ReduceScore(1);
            players[targetPlayerIndex].GainTagAbility();
            players[playerIndex].LoseTagAbility();
        }
        if (gameMode == GameMode.Infection)
        {

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

    public void GainTagAbility()
    {
        tagAbility = true;
    }

    public void LoseTagAbility()
    {
        tagAbility = false;
    }

}
