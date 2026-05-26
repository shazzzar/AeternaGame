using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.clip = backgroundMusic;
audioSource.loop = true;
            audioSource.playOnAwake = true;
            
            // Load saved music volume or default to 1
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            audioSource.volume = savedVolume;

            // Apply master volume globally
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            
            if (backgroundMusic != null)
{
                audioSource.Play();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void ChangeMusic(AudioClip newClip)
    {
        if (audioSource.clip == newClip) return;
        
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }
    }
