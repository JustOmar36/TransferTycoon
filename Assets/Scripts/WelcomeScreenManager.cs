using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WelcomeScreenManager : MonoBehaviour
{
    public GameObject nameDifficultyPanel; // Panel with input and difficulty
    public Button startButton;
    public TMP_InputField nameInputField;

    public Toggle easyToggle;
    public Toggle mediumToggle;
    public Toggle hardToggle;

    public Button confirmButton;

    private string playerName;
    private string difficulty;

    void Start()
    {
        nameDifficultyPanel.SetActive(false);
        startButton.onClick.AddListener(OnStartClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    void OnStartClicked()
    {
        SceneManager.LoadScene("GameplayScene");
        startButton.gameObject.SetActive(false);
        nameDifficultyPanel.SetActive(true);
    }

    void OnConfirmClicked()
    {
        playerName = nameInputField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Please enter a name.");
            return;
        }

        if (easyToggle.isOn)
            difficulty = "Easy";
        else if (mediumToggle.isOn)
            difficulty = "Medium";
        else if (hardToggle.isOn)
            difficulty = "Hard";
        else
        {
            Debug.LogWarning("Please select a difficulty.");
            return;
        }

        Debug.Log($"Player Name: {playerName}, Difficulty: {difficulty}");
        // Proceed to load your game scene or start gameplay.
        // Example:
        // SceneManager.LoadScene("GameScene");
    }
}
