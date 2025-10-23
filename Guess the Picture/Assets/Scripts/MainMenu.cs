using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SpriteScoreDisplay roundNumDisplay;

    [SerializeField]
    private GameObject titleText, numRoundsImage;

    [SerializeField]
    private GameObject startButton, quitButton, backButton, beginGame;

    [SerializeField]
    private Slider numOfRoundsSlider;

    public static int NumOfRounds = 1;

    private void Start()
    {
        titleText.SetActive(true);
        startButton.SetActive(true);
        quitButton.SetActive(true);

        beginGame.SetActive(false);
        //numRoundsText.SetActive(false);
        numOfRoundsSlider.gameObject.SetActive(false);
        backButton.SetActive(false);
        numRoundsImage.SetActive(false);
    }

    public void OnStartButtonPressed()
    {
        // Load the num of rounds selection menu
        titleText.SetActive(false);
        startButton.SetActive(false);
        quitButton.SetActive(false);

        backButton.SetActive(true);
        //numRoundsText.SetActive(true);
        numRoundsImage.SetActive(true);
        numOfRoundsSlider.gameObject.SetActive(true);
        beginGame.SetActive(true);
        roundNumDisplay.SetDisplay("1");
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("quit");
        Application.Quit();
    }

    public void OnNumOfRoundsSliderChanged()
    {
        NumOfRounds = (int)numOfRoundsSlider.value;
        roundNumDisplay.SetDisplay(NumOfRounds.ToString());
    }

    public void OnNumOfRoundsSelected()
    {
        // Start the game with the selected number of rounds
        // For example, load the game scene here
        Debug.Log("Starting game with " + NumOfRounds + " rounds.");
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void OnBackButtonPressed()
    {
        // Return to the main menu
        titleText.SetActive(true);
        startButton.SetActive(true);
        quitButton.SetActive(true);

        numRoundsImage.SetActive(false);
        //numRoundsText.SetActive(false);
        numOfRoundsSlider.gameObject.SetActive(false);
        beginGame.SetActive(false);
        backButton.SetActive(false);
    }
}
