using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool ShouldQuitGame => Input.GetKeyUp(KeyCode.Escape);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldQuitGame)
        {
            QuitGame();
        }
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
