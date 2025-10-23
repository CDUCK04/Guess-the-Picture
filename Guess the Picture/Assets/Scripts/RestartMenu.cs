using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartMenu : MonoBehaviour
{

    [SerializeField] private GameObject startButton, backButton;

    int currentSceneIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.SetActive(false);
        backButton.SetActive(false);
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    public void ShowRestartMenu()
    {
        startButton.SetActive(true);
        backButton.SetActive(true);
    }

    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene(currentSceneIndex - 1);
    }
}
