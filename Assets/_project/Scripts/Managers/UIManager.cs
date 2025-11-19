using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{   
    public static UIManager Instance;

    [SerializeField] TargetIndicator _targetIndicatorPrefab;
    [SerializeField] Canvas _mainCanvas;
    [SerializeField] TMP_Text _scoreText, _highScoreText;
    [SerializeField] GameObject _gameOverScreen;

    List<TargetIndicator> _targetIndicators;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _targetIndicators = new List<TargetIndicator>();
    }

    void OnEnable()
    {
        SubscribeToEvents();
        if (_gameOverScreen != null)
            _gameOverScreen.SetActive(false);
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void Start()
    {
        // Subscribe again in Start to catch managers that weren't ready in OnEnable
        SubscribeToEvents();
    }

    public void AddTarget(Transform target)
    {
        if (_targetIndicatorPrefab == null || _mainCanvas == null) return;
        
        var targetIndicator = Instantiate(_targetIndicatorPrefab, _mainCanvas.transform);
        targetIndicator.Init(target, _mainCanvas);
        _targetIndicators.Add(targetIndicator);
    }

    public void RemoveTarget(Transform target)
    {
        if (target == null) return;
        
        var key = target.GetInstanceID();
        var indicator = _targetIndicators.FirstOrDefault(i => i != null && i.Key == key);
        if (indicator) 
        {
            _targetIndicators.Remove(indicator);
            Destroy(indicator.gameObject);
        }
    }

    public void UpdateTargetIndicators(List<Transform> targets, int lockedOnTarget)
    {
        if (targets == null) return;
        
        // Clean up any null indicators first
        _targetIndicators.RemoveAll(i => i == null);
        
        foreach (var targetIndicator in _targetIndicators)
        {
            if (targetIndicator == null) continue;
            
            targetIndicator.gameObject.SetActive(targets.Any(target => target != null && target.GetInstanceID() == targetIndicator.Key));
            targetIndicator.LockedOn = targetIndicator.Key == lockedOnTarget;
        }
    }

    void SubscribeToEvents()
    {
        SubscribeToScoreManagerEvents();
        SubscribeToGameManagerEvents();
    }
    
    void UnsubscribeFromEvents()
    {
        UnsubscribeFromScoreManagerEvents();
        UnsubscribeFromGameManagerEvents();
    }
    
    void SubscribeToGameManagerEvents()
    {
        // Check if GameManager exists before subscribing
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null in SubscribeToGameManagerEvents - skipping subscription");
            return;
        }
        
        // Unsubscribe first to prevent duplicate subscriptions
        UnsubscribeFromGameManagerEvents();
        GameManager.Instance.GameStateChanged += OnGameStateChanged;
    }
    
    void UnsubscribeFromGameManagerEvents()
    {
        // Check if GameManager exists before unsubscribing
        if (GameManager.Instance == null) return;
        
        GameManager.Instance.GameStateChanged -= OnGameStateChanged;
    }
    
    void SubscribeToScoreManagerEvents()
    {
        if (ScoreManager.Instance == null) return;
        
        // Unsubscribe first to prevent duplicate subscriptions
        UnsubscribeFromScoreManagerEvents();
        
        ScoreManager.Instance.ScoreChanged += OnScoreChanged;
        ScoreManager.Instance.HighScoreChanged += OnHighScoreChanged;
    }
    
    void UnsubscribeFromScoreManagerEvents()
    {
        if (ScoreManager.Instance == null) return;
        
        ScoreManager.Instance.ScoreChanged -= OnScoreChanged;
        ScoreManager.Instance.HighScoreChanged -= OnHighScoreChanged;
    }

    void OnGameStateChanged(GameState gameState)
    {
        if (_gameOverScreen != null)
            _gameOverScreen.SetActive(gameState == GameState.GameOver);
    }

    void OnScoreChanged(int score)
    {
        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }

    void OnHighScoreChanged(int highScore)
    {
        if (_highScoreText != null)
            _highScoreText.text = highScore.ToString();
    }
}