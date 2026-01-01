using UnityEngine;

public enum GameMode
{
    Tag,
    Infection
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("GameSettings")]
    public int playerAmount = 0;
    public GameMode gameMode;
    public string map;

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
