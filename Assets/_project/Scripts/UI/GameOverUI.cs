using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;

    private void OnEnable()
    {
        // When this canvas is activated, update the score fields
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance == null)
        {
            finalScoreText.text = "Score: 0";
            highScoreText.text = "High Score: 0";
            return;
        }

        finalScoreText.text = "Score: " + ScoreManager.Instance.Score;
        highScoreText.text = "High Score: " + ScoreManager.Instance.HighScore;
    }
}
