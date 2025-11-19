using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<GameState> GameStateChanged = delegate { };

    public GameState GameState { get; private set; }

    bool ShouldQuitGame => Input.GetKeyUp(KeyCode.Escape);

    void Awake()
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
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        SetGameState(GameState.Patrol);
        //MusicManager.Instance.PlayPatrolMusic();
    }

    void Update()
    {
        if (ShouldQuitGame)
        {
            QuitGame();
        }
    }

    public void InCombat(bool inCombat)
    {
        if (GameState == GameState.Combat) return;

        if (inCombat)
        {
            MusicManager.Instance.PlayCombatMusic();
            SetGameState(GameState.Combat);
            return;
        }

        MusicManager.Instance.PlayPatrolMusic();
    }

    public void PlayerWon()
    {
        MusicManager.Instance.PlayGameOverMusic();
        SetGameState(GameState.GameOver);
        Debug.Log("Player Won!");
    }

    public void PlayerLost()
    {
        // When the player dies (but is disabled), just update game state
        MusicManager.Instance.PlayGameOverMusic();
        SetGameState(GameState.GameOver);
        Debug.Log("Player Lost!");
    }

    void SetGameState(GameState gameState)
    {
        if (gameState == GameState) return;
        GameState = gameState;
        GameStateChanged(gameState);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
