using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject mainMenuPanel;   // Painel principal
    public GameObject optionsPanel;    // Painel de opções
    public GameObject quitConfirmPanel; // (Opcional) Painel de confirmação de saída

    // Botão "Start"
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Botão "Options"
    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
    public void BackToMenu()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }


    // Botão "Quit"
    public void QuitGame()
    {
        
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }