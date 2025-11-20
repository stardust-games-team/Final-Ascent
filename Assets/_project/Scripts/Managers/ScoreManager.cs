using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public event Action<int> ScoreChanged = delegate { };
    public event Action<int> HighScoreChanged = delegate { };

    public int Score { get; private set; }
    public int HighScore { get; private set; }

    const string HighScoreKey = "HighScore";

    static Queue<int> _pendingPoints = new Queue<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Process any points that were queued before ScoreManager existed
        while (_pendingPoints.Count > 0)
        {
            AddPoints(_pendingPoints.Dequeue());
        }
    }

    void Start(){
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        HighScoreChanged(HighScore);
    }

    void OnDisable()
    {
        PlayerPrefs.SetInt(HighScoreKey, HighScore);
    }

    public void ResetScore()
    {
        Score = 0;
        ScoreChanged(Score);
    }
    public void ResetHighScore()
{
    PlayerPrefs.SetInt(HighScoreKey, 0);
    HighScore = 0;
    HighScoreChanged(HighScore);
    Debug.Log("High Score reset!");
}

    void Update()
{
    if (Input.GetKeyDown(KeyCode.R))
    {
        ResetHighScore();
    }
}


    public void AddPoints(int points)
    {
        if (GameManager.Instance.GameState == GameState.GameOver) return;
        Score += points;
        ScoreChanged(Score);

        if (Score > HighScore)
        {
            HighScore = Score;
            HighScoreChanged(HighScore);
        }
    }

    // Called by AddPointsWhenDestroyed if ScoreManager isn't ready yet
    public static void QueuePoints(int points)
    {
        _pendingPoints.Enqueue(points);
    }
}
