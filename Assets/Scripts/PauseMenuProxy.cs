using UnityEngine;

public class PauseMenuProxy : MonoBehaviour
{
    public void Resume()
    {
        PauseMenu pm = FindActivePauseMenu();
        if (pm != null) pm.Resume();
    }

    public void QuitGame()
    {
        PauseMenu pm = FindActivePauseMenu();
        if (pm != null) pm.QuitGame();
    }

    private PauseMenu FindActivePauseMenu()
    {
        if (RoundManager.Instance != null)
        {
            return RoundManager.Instance.GetComponent<PauseMenu>();
        }
        return Object.FindAnyObjectByType<PauseMenu>();
    }
}
