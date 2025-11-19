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

    public void ResetScore()
    {
        Score = 0;
        ScoreChanged(Score);
    }

    public void AddPoints(int points)
    {
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
