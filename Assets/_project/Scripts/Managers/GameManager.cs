using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
        MusicManager.Instance.PlayPatrolMusic();
    }

    void Update()
    {
        if (ShouldQuitGame)
        {
            QuitGame();
        }
    }

    public void InCombat (bool inCombat)
    {
        if (inCombat)
        {
            MusicManager.Instance.PlayCombatMusic();
            return;
        }

        MusicManager.Instance.PlayPatrolMusic();
    }

    void QuitGame()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
    #else
            // todo handle WebGL
           Application.Quit();
    #endif
    }
}
