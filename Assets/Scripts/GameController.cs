using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    // ***********************
    // Data asset and configuration
    // ***********************
    private Data dataAsset;
    private const string dataAssetPath = "Assets/Resources/Data.asset";

    // ***********************
    // UI Elements
    // ***********************
    [Header("Prefabs and Containers")]
    public GameObject optionPrefab;
    // Container for available (original) option buttons
    public Transform optionContainer;
    // Container for selected option buttons (should use a GridLayoutGroup)
    public Transform SelectedOptionsContainer;

    [Header("Question & Progress UI")]
    public GameObject questionImage;
    public Text CurrentLevel;
    public Text lastLevel;
    public GameObject progressBar;
    public Text textWarning;

    // ***********************
    // Option Button Lists
    // ***********************
    // List to keep track of the original option buttons available for selection
    private List<OptionButton> originalOptions = new List<OptionButton>();
    // List to keep track of the option buttons in the order they are clicked
    private List<OptionButton> selectedOptions = new List<OptionButton>();

    // ***********************
    // Lock flag for wrong answer feedback
    // ***********************
    private bool isAnswerLocked = false;

    // ***********************
    // Unity Lifecycle Methods
    // ***********************

    public void Awake()
    {
        dataAsset = Resources.Load<Data>("Data");
        if (dataAsset != null)
        {
            // Set the starting level and update UI text for the final level count
            dataAsset.currentLevelIndex = 0;
            lastLevel.text = "Level " + dataAsset.Levels.Count.ToString();

            // Initialize the progress bar (fill amount starts at 0)
            Image imgProgressBar = progressBar.GetComponent<Image>();
            imgProgressBar.fillAmount = 0;
        }
        else
        {
            Debug.LogWarning("Data asset not found.");
            return;
        }
    }

    public void Start()
    {
        textWarning.text = "";

        // Load words for the current level from the data asset
        if (dataAsset != null && dataAsset.Levels != null && dataAsset.Levels.Count > dataAsset.currentLevelIndex)
        {
            List<string> words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);

            // Set the question image to the level's question sprite
            if (questionImage != null)
            {
                Image img = questionImage.GetComponent<Image>();
                if (img != null)
                    img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
                else
                    Debug.LogWarning("No Image component found on questionImage GameObject.");
            }

            // Shuffle and create option buttons (this also initializes the originalOptions list)
            RandomizeList(words);
            CreateButtons(words);
        }
        else
        {
            Debug.LogWarning("Data asset not found or level data is missing.");
        }
    }

    // ***********************
    // Utility Methods
    // ***********************

    // Shuffles a list in place using the Fisher–Yates algorithm
    private void RandomizeList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // ***********************
    // Button Creation & Management
    // ***********************

    // Creates buttons for each word and initializes the originalOptions list.
    public void CreateButtons(List<string> words)
    {
        // Clear previous option buttons from both containers
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in SelectedOptionsContainer)
        {
            Destroy(child.gameObject);
        }
        // Reset lists tracking options
        originalOptions.Clear();
        selectedOptions.Clear();

        // Instantiate a button for each word in the list
        foreach (string word in words)
        {
            GameObject newButton = Instantiate(optionPrefab, optionContainer);
            OptionButton optionBtn = newButton.GetComponent<OptionButton>();
            if (optionBtn != null)
            {
                // Initialize the button with this manager and the word text
                optionBtn.Initialize(this, word);
                // Add to the list of original option buttons
                originalOptions.Add(optionBtn);
            }
            else
            {
                Debug.LogWarning("OptionButton component missing on prefab.");
            }
            // Set up the click event for the button if it has a Button component
            Button btn = newButton.GetComponent<Button>();
            if (btn != null && optionBtn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(optionBtn.OnButtonClicked);
            }
        }
    }

    // Called by an OptionButton when clicked to handle selection.
    // Once an option is selected, it is moved to the SelectedOptionsContainer and disabled.
    public void HandleOptionButtonClick(OptionButton btn)
    {
        // If the button is already selected, remove it (to update its position) before re-adding it.
        if (selectedOptions.Contains(btn))
        {
            selectedOptions.Remove(btn);
        }
        selectedOptions.Add(btn);

        // Update the display order based on the current order in the selectedOptions list.
        UpdateSelectedOptionsDisplay();

        // Move the button to the selected options container.
        btn.transform.SetParent(SelectedOptionsContainer, false);

        // Disable the button so it cannot be clicked again.
        Button btnComponent = btn.GetComponent<Button>();
        if (btnComponent != null)
            btnComponent.interactable = false;
    }

    // Updates the UI display for selected options based on their order in the list.
    private void UpdateSelectedOptionsDisplay()
    {
        for (int i = 0; i < selectedOptions.Count; i++)
        {
            // Set the index display according to the position in the list (i+1)
            selectedOptions[i].SetIndex(i + 1);
        }
    }

    // Called when the Submit button is clicked.
    public void SubmitAnswer()
    {
        Debug.Log("User pressed Submit button");

        // Check if all option buttons have been selected.
        int totalOptions = originalOptions.Count;
        if (selectedOptions.Count < totalOptions)
        {
            Debug.Log("Please select all words.");
            textWarning.text = "Please select all words before Submitting";
            StartCoroutine(ClearWarningAfterDelay(3f));
            return;
        }

        List<string> userOrder = new List<string>();
        // Use the order of selectedOptions directly.
        foreach (OptionButton btn in selectedOptions)
        {
            userOrder.Add(btn.wordText.text);
        }

        // Get the correct answer order from the Data asset.
        List<string> correctOrder = dataAsset.Levels[dataAsset.currentLevelIndex].words;

        // Compare the sentences.
        string userSentence = string.Join(" ", userOrder);
        string correctSentence = string.Join(" ", correctOrder);

        bool isCorrect = userSentence == correctSentence;

        if (isCorrect)
        {
            Debug.Log("Correct answer! Moving to next level...");
            MoveToNextLevel();
        }
        else
        {
            Debug.Log("Wrong answer! Disabling reset and showing wrong answer sprite.");
            StartCoroutine(WrongAnswerSequence());
        }
    }

    // Called when the Reset button is pressed to clear current selections and reset the options.
    public void ResetAnswer()
    {
        if (isAnswerLocked)
        {
            Debug.Log("Reset is disabled while wrong answer feedback is active.");
            return;
        }

        Debug.Log("User pressed Reset Button");

        // Reload the words for the current level and reinitialize the buttons.
        if (dataAsset != null && dataAsset.Levels != null && dataAsset.Levels.Count > dataAsset.currentLevelIndex)
        {
            List<string> words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);
            RandomizeList(words);
            CreateButtons(words);
        }
        else
        {
            Debug.LogWarning("Unable to reset: Level data not found.");
        }
    }

    // ***********************
    // Level Progression & Feedback
    // ***********************

    // Advances to the next level and updates the UI accordingly.
    private void MoveToNextLevel()
    {
        if (dataAsset != null && dataAsset.Levels != null)
        {
            // Update the progress bar based on level progress.
            Image imgProgressBar = progressBar.GetComponent<Image>();
            imgProgressBar.fillAmount = (float)(dataAsset.currentLevelIndex + 1) / dataAsset.Levels.Count;
            Debug.Log("Fill amount " + imgProgressBar.fillAmount);

            if (dataAsset.currentLevelIndex < dataAsset.Levels.Count - 1)
            {
                dataAsset.currentLevelIndex++;
                CurrentLevel.text = "Level " + (dataAsset.currentLevelIndex + 1).ToString();

                // Update the question image for the new level.
                if (questionImage != null)
                {
                    Image img = questionImage.GetComponent<Image>();
                    if (img != null)
                        img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
                }
                // Create new option buttons for the next level.
                List<string> words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);
                RandomizeList(words);
                CreateButtons(words);
            }
            else
            {
                Debug.Log("Quiz complete!");
                ShowQuizCompleteDialog();
            }
        }
    }

    // Displays the wrong answer sprite briefly before resetting the answer.
    private IEnumerator WrongAnswerSequence()
    {
        // Lock the answer reset function while feedback is active.
        isAnswerLocked = true;

        // Change the question image to the wrong answer sprite.
        if (questionImage != null)
        {
            Image img = questionImage.GetComponent<Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].wrongAnswerSprite;
        }

        yield return new WaitForSeconds(5f);

        // Revert the image back to the correct question sprite.
        if (questionImage != null)
        {
            Image img = questionImage.GetComponent<Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
        }

        // Unlock the reset function.
        isAnswerLocked = false;

        // Reset the option selections after showing feedback.
        ResetAnswer();
    }

    // Shows a dialog indicating the quiz is complete and quits the application.
    private void ShowQuizCompleteDialog()
    {
        Debug.Log("Quiz complete! Exiting game...");
        textWarning.text = "Congratulations You Won!!!";
        StartCoroutine(ClearWarningAfterDelay(3f));
        Application.Quit();
    }

    // Clears the warning text after a specified delay.
    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textWarning.text = "";
    }
}
