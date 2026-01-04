using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("GameSettings")]
    public int playerAmount = 0;
    public GameMode gameMode;
    public string map;

    [Header("Tag Settings")]
    public int tagLives;
    public int tagTime;

    [Header("Infection Settings")]
    public int rounds;
    public int roundDuration;
    public int laststandTime;

    [Header("Bomb Settings")]
    public int fuseTime;
    public int bombLives;
    public int bombRounds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
