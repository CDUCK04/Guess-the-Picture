using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartMenu : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxBeep;

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    [SerializeField] private GameObject startButton, backButton;

    int currentSceneIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    public void ShowRestartMenu()
    {
        startButton.SetActive(true);
        backButton.SetActive(true);
    }

    public void OnStartButtonPressed()
    {
        PlaySFX(sfxBeep);
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void OnBackButtonPressed()
    {
        PlaySFX(sfxBeep);
        SceneManager.LoadScene(currentSceneIndex - 1);
    }
}
