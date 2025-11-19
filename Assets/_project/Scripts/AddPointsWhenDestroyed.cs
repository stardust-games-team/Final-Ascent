using UnityEngine;

public class AddPointsWhenDestroyed : MonoBehaviour
{
    [SerializeField] int _points = 20;   // Points to add when this object is destroyed
    bool _scored;                         // Ensures points are only added once

    // Called when the object is destroyed (GameObject removed)
    void OnDestroy()
    {
        AddScore();
    }

    // Called when the object is disabled (e.g., set inactive)
    void OnDisable()
    {
        AddScore();
    }

    /// <summary>
    /// Add points to the ScoreManager, if possible.
    /// - Only adds points once per object.
    /// - Queues points if ScoreManager isn't ready yet (avoids null errors during startup).
    /// - Does not count points for the player object or any other excluded objects.
    /// </summary>
    void AddScore()
    {
        if (_scored) return;   // Skip if already added
        _scored = true;

        // Check if ScoreManager exists in the scene
        if (ScoreManager.Instance != null)
        {
            // Safely add points
            ScoreManager.Instance.AddPoints(_points);
            Debug.Log($"Added {_points} points. ScoreManager exists.");
        }
        else
        {
            // If ScoreManager isn't ready yet:
            // 1. We can't add points immediately (avoids null reference errors)
            // 2. Queue the points for later (so they aren't lost)
            // 3. This usually happens if this object is destroyed before ScoreManager Awake()
            ScoreManager.QueuePoints(_points);

            // Optional: you can comment out the warning if you don't want spam in the console
            Debug.LogWarning("ScoreManager not ready, queued points for later.");
        }
    }
}
