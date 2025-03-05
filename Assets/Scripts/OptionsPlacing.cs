using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class OptionsPlacing : MonoBehaviour
{
    // Reference to the Data asset that stores our quiz info
    private Data dataAsset;

    // Prefab for an option button (should have the OptionButton component)
    public GameObject optionPrefab;

    // Container where the option buttons will be placed (like a vertical list)
    public Transform optionContainer;

    // GameObject holding the question image (also used for wrong answer feedback)
    public GameObject questionImage;

    // Path to the Data asset in the project
    private const string dataAssetPath = "Assets/ScriptableObjects/Data.asset";

    // UI elements for showing current and last level information
    public UnityEngine.UI.Text CurrentLevel;
    public UnityEngine.UI.Text lastLevel;

    // Progress bar object to show quiz progress
    public GameObject progressBar;

    // Warning text to display messages to the user
    public UnityEngine.UI.Text textWarning;

    // List to keep track of the option buttons in the order they are clicked
    private List<OptionButton> selectedButtons = new List<OptionButton>();

    public void Start()
    {
        // Create a local list to hold the words for the current level
        List<string> words = new List<string>();

#if UNITY_EDITOR
        // Load the Data asset from the editor folder
        dataAsset = AssetDatabase.LoadAssetAtPath<Data>(dataAssetPath);
        if (dataAsset != null)
        {
            // Set the starting level and update UI text for the final level count
            dataAsset.currentLevelIndex = 0;
            lastLevel.text = "Level " + dataAsset.Levels.Count.ToString();

            // Initialize the progress bar (fill amount starts at 0)
            UnityEngine.UI.Image imgProgressBar = progressBar.GetComponent<UnityEngine.UI.Image>();
            imgProgressBar.fillAmount = 0;
        }
        else
        {
            Debug.LogWarning("Data asset not found.");
            return;
        }
#else
        Debug.LogError("AssetDatabase is only available in the Editor. Use Resources.Load for runtime.");
        return;
#endif

        textWarning.text = "";

        // If the data asset and level data are valid, copy the words for the current level
        if (dataAsset != null && dataAsset.Levels != null && dataAsset.Levels.Count > dataAsset.currentLevelIndex)
        {
            words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);

            // Set the question image to the level's question sprite
            if (questionImage != null)
            {
                UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
                else
                    Debug.LogWarning("No Image component found on questionImage GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("Data asset not found or level data is missing.");
        }

        // Shuffle the words list without affecting the original asset
        RandomizeList(words);

        // Create option buttons using the randomized list
        CreateButtons(words);
    }

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

    // Creates a button for each word in the list
    public void CreateButtons(List<string> words)
    {
        // Remove any existing buttons from the container
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
        selectedButtons.Clear();

        // Create a new button for each word
        foreach (string word in words)
        {
            GameObject newButton = Instantiate(optionPrefab, optionContainer);
            OptionButton optionBtn = newButton.GetComponent<OptionButton>();
            if (optionBtn != null)
            {
                // Initialize the button with this manager and the word text
                optionBtn.Initialize(this, word);
            }
            else
            {
                Debug.LogWarning("OptionButton component missing on prefab.");
            }
            // Set up the click event for the button if it has a Button component
            UnityEngine.UI.Button btn = newButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null && optionBtn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(optionBtn.OnButtonClicked);
            }
        }
    }

    // Called by an OptionButton when clicked to track selection order
    public void HandleOptionButtonClick(OptionButton btn)
    {
        // If the button hasn't been selected yet (its index is 0)
        if (btn.currentIndex == 0)
        {
            selectedButtons.Add(btn);
            btn.SetIndex(selectedButtons.Count);
        }
        else
        {
            // If already selected, remove and re-add it to update its position
            selectedButtons.Remove(btn);
            selectedButtons.Add(btn);
            // Update indices for all selected buttons
            for (int i = 0; i < selectedButtons.Count; i++)
            {
                selectedButtons[i].SetIndex(i + 1);
            }
        }
    }

    // Called when the Submit button is clicked
    public void SubmitAnswer()
    {
        Debug.Log("User pressed Submit button");

        // Check if all option buttons have been selected
        int totalOptions = optionContainer.childCount;
        if (selectedButtons.Count < totalOptions)
        {
            Debug.Log("Please select all words.");
            textWarning.text = "Please select all words before Submitting";
            StartCoroutine(ClearWarningAfterDelay(3f));
            return;
        }

        // Build the user's answer list based on the selection order
        List<string> userOrder = new List<string>();
        selectedButtons.Sort((a, b) => a.currentIndex.CompareTo(b.currentIndex));
        foreach (OptionButton btn in selectedButtons)
        {
            // Assumes each button has a Text component showing the word
            userOrder.Add(btn.wordText.text);
        }

        // Get the correct answer order from the Data asset
        List<string> correctOrder = dataAsset.Levels[dataAsset.currentLevelIndex].words;

        // Compare the user's order to the correct order
        bool isCorrect = true;
        if (userOrder.Count != correctOrder.Count)
        {
            isCorrect = false;
        }
        else
        {
            for (int i = 0; i < userOrder.Count; i++)
            {
                if (userOrder[i] != correctOrder[i])
                {
                    isCorrect = false;
                    break;
                }
            }
        }

        // If the answer is correct, move to the next level; otherwise, show feedback
        if (isCorrect)
        {
            Debug.Log("Correct answer! Moving to next level...");
            MoveToNextLevel();
        }
        else
        {
            Debug.Log("Wrong answer! Resetting after showing wrong answer sprite.");
            StartCoroutine(WrongAnswerSequence());
        }
    }

    // Called when the Reset button is pressed to clear current selections
    public void ResetAnswer()
    {
        Debug.Log("User pressed Reset Button");
        selectedButtons.Clear();

        // Get the words for the current level and randomize them
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

    // Advances to the next level by updating the UI and reloading buttons
    private void MoveToNextLevel()
    {
        if (dataAsset != null && dataAsset.Levels != null)
        {
            // Update the progress bar fill amount based on level progress
            UnityEngine.UI.Image imgProgressBar = progressBar.GetComponent<UnityEngine.UI.Image>();
            imgProgressBar.fillAmount = (float)(dataAsset.currentLevelIndex + 1) / dataAsset.Levels.Count;
            Debug.Log("Fill amount " + imgProgressBar.fillAmount);

            if (dataAsset.currentLevelIndex < dataAsset.Levels.Count - 1)
            {
                dataAsset.currentLevelIndex++;
                CurrentLevel.text = "Level " + (dataAsset.currentLevelIndex + 1).ToString();

                // Update the question image for the new level
                if (questionImage != null)
                {
                    UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                        img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
                }
                // Create new option buttons for the next level
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

    // Shows the wrong answer sprite for a few seconds before resetting the selection
    private IEnumerator WrongAnswerSequence()
    {
        // Change the question image to the wrong answer sprite
        if (questionImage != null)
        {
            UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].wrongAnswerSprite;
        }

        yield return new WaitForSeconds(5f);

        // Revert the image back to the correct question sprite
        if (questionImage != null)
        {
            UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
        }

        // Reset the option selections after showing feedback
        ResetAnswer();
    }

    // Shows a dialog indicating the quiz is complete and quits the application
    private void ShowQuizCompleteDialog()
    {
        Debug.Log("Quiz complete! Exiting game...");
        Application.Quit();
    }

    // Clears the warning text after a short delay
    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textWarning.text = "";
    }
}
