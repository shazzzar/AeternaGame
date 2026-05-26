using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't pause if other important UI is open (like shop)
            if (RoundManager.Instance != null && RoundManager.Instance.isShopPhase) return;

            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (RoundManager.Instance != null && RoundManager.Instance.pausePanel != null)
            RoundManager.Instance.pausePanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
        
        UpdateCursorState();
    }

    public void Pause()
    {
        if (RoundManager.Instance != null && RoundManager.Instance.pausePanel != null)
            RoundManager.Instance.pausePanel.SetActive(true);

        Time.timeScale = 0f;
        IsPaused = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateCursorState()
    {
        PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.UpdateCursorState();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
