using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject mainMenuPanel;   // Painel principal
    public GameObject optionsPanel;    // Painel de opções
    public GameObject quitConfirmPanel; // (Opcional) Painel de confirmação de saída

    [Header("Volume Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    private void Start()
    {
        // Load saved volumes or default to 1
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Apply initially
        AudioListener.volume = masterVol;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(musicVol);
        }
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(value);
        }
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

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