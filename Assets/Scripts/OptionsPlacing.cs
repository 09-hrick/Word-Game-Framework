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
    private Data dataAsset;
    public GameObject optionPrefab;       // Prefab for an option button (must have OptionButton component)
    public Transform optionContainer;     // Parent container (e.g., with a Vertical Layout Group)
    public GameObject questionImage;      // Image to display the question (and wrong answer feedback)
    private const string dataAssetPath = "Assets/ScriptableObjects/Data.asset";
    public UnityEngine.UI.Text CurrentLevel;
    public UnityEngine.UI.Text lastLevel;
    public GameObject progressBar;
    public UnityEngine.UI.Text textWarning;


    // List to track the order in which option buttons are clicked (their selection order).
    private List<OptionButton> selectedButtons = new List<OptionButton>();

    public void Start()
    {
        // Local copy for the words.
        List<string> words = new List<string>();

#if UNITY_EDITOR
    // Load the Data asset first.
    dataAsset = AssetDatabase.LoadAssetAtPath<Data>(dataAssetPath);
    if (dataAsset != null)
    {
        // Now that dataAsset is loaded, set the current level.
        dataAsset.currentLevelIndex = 0;
        lastLevel.text = "Level " + dataAsset.Levels.Count.ToString();    
        UnityEngine.UI.Image imgProgressBar = progressBar.GetComponent<UnityEngine.UI.Image>();
        imgProgressBar.fillAmount= 0;
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

        // If the data asset was successfully loaded, create a local copy of the words for the desired level.
        if (dataAsset != null && dataAsset.Levels != null && dataAsset.Levels.Count > dataAsset.currentLevelIndex)
        {
            words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);
            // Set the question image to the correct question sprite.
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

        // Randomize the local copy without modifying the original asset data.
        RandomizeList(words);

        CreateButtons(words);
    }

    // Generic helper function to randomize a list using Fisher–Yates shuffle.
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

    public void CreateButtons(List<string> words)
    {
        // Remove existing option buttons.
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
        selectedButtons.Clear();

        // Instantiate an option button for each word.
        foreach (string word in words)
        {
            GameObject newButton = Instantiate(optionPrefab, optionContainer);
            OptionButton optionBtn = newButton.GetComponent<OptionButton>();
            if (optionBtn != null)
            {
                // Initialize the button with this manager and the word.
                optionBtn.Initialize(this, word);
            }
            else
            {
                Debug.LogWarning("OptionButton component missing on prefab.");
            }
            // Hook up the button click event if a Button component exists.
            UnityEngine.UI.Button btn = newButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null && optionBtn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(optionBtn.OnButtonClicked);
            }
        }
    }

    /// <summary>
    /// Called by an OptionButton when it is clicked.
    /// </summary>
    public void HandleOptionButtonClick(OptionButton btn)
    {
        // If the button hasn't been selected yet (index is 0)
        if (btn.currentIndex == 0)
        {
            selectedButtons.Add(btn);
            btn.SetIndex(selectedButtons.Count);
        }
        else
        {
            // If the button is already selected,
            // remove it from its current position and add it at the end.
            selectedButtons.Remove(btn);
            selectedButtons.Add(btn);
            // Update indices for all selected buttons.
            for (int i = 0; i < selectedButtons.Count; i++)
            {
                selectedButtons[i].SetIndex(i + 1);
            }
        }
    }

    /// <summary>
    /// Called when the Submit button is clicked.
    /// Checks if all options have been selected, then compares the order.
    /// </summary>
    public void SubmitAnswer()
    {
        Debug.Log("User pressed Submit button");
        // Ensure all option buttons are selected.
        int totalOptions = optionContainer.childCount;
        if (selectedButtons.Count < totalOptions)
        {
            Debug.Log("Please select all words.");
            textWarning.text = "Please select all words before Submitting";
            StartCoroutine(ClearWarningAfterDelay(3f));
            return;
        }

        // Build the user answer list (sorted by the order assigned to each button).
        List<string> userOrder = new List<string>();
        // It's a good idea to sort the selectedButtons by their current index.
        selectedButtons.Sort((a, b) => a.currentIndex.CompareTo(b.currentIndex));
        foreach (OptionButton btn in selectedButtons)
        {
            // Assuming the OptionButton stores the displayed word in its child Text (wordText).
            userOrder.Add(btn.wordText.text);
        }

        // Get the correct answer order from the Data asset.
        List<string> correctOrder = dataAsset.Levels[dataAsset.currentLevelIndex].words;

        // Compare user order with the correct order.
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

    /// <summary>
    /// Called when the Reset button is clicked.
    /// Resets all option button indices.
    /// </summary>
    public void ResetAnswer()
    {
        Debug.Log("User pressed Reset Button");
        // Clear the current selections.
        selectedButtons.Clear();

        // Get the words for the current level.
        if (dataAsset != null && dataAsset.Levels != null && dataAsset.Levels.Count > dataAsset.currentLevelIndex)
        {
            List<string> words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);

            // Randomize the words.
            RandomizeList(words);

            // Re-create the option buttons with the new randomized order.
            CreateButtons(words);
        }
        else
        {
            Debug.LogWarning("Unable to reset: Level data not found.");
        }
    }

    /// <summary>
    /// Advances to the next level.
    /// Updates the question image and recreates the option buttons.
    /// </summary>
    private void MoveToNextLevel()
    {
        // Increment the level index (ensure you don't go out-of-range).
        if (dataAsset != null && dataAsset.Levels != null)
        {
            UnityEngine.UI.Image imgProgressBar = progressBar.GetComponent<UnityEngine.UI.Image>();
            imgProgressBar.fillAmount = (float)(dataAsset.currentLevelIndex+1) / dataAsset.Levels.Count;
            Debug.Log("Fill amount " + imgProgressBar.fillAmount);
            if (dataAsset.currentLevelIndex < dataAsset.Levels.Count - 1)
            {
                dataAsset.currentLevelIndex++;
                CurrentLevel.text = "Level " + (dataAsset.currentLevelIndex+1).ToString();
                // Update question image.
                if (questionImage != null)
                {
                    UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                        img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
                }
                // Re-create buttons with the new level's words.
                List<string> words = new List<string>(dataAsset.Levels[dataAsset.currentLevelIndex].words);
                RandomizeList(words);
                CreateButtons(words);
            }
            else
            {
                Debug.Log("Quiz complete!");
                // Show dialog and then exit.
                ShowQuizCompleteDialog();
            }
        }
    }

    /// <summary>
    /// Shows the wrong answer sprite for 5 seconds, then reverts back and resets the selection.
    /// </summary>
    private IEnumerator WrongAnswerSequence()
    {
        // Change the question image to the wrong answer sprite.
        if (questionImage != null)
        {
            UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].wrongAnswerSprite;
        }

        yield return new WaitForSeconds(5f);

        // Revert back to the correct question sprite.
        if (questionImage != null)
        {
            UnityEngine.UI.Image img = questionImage.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.sprite = dataAsset.Levels[dataAsset.currentLevelIndex].questionSprite;
        }

        // Reset the selected options.
        ResetAnswer();
    }
    /// <summary>
    /// Displays a dialog box indicating the quiz is complete and exits the game.
    /// </summary>
    private void ShowQuizCompleteDialog()
    {
        Debug.Log("Quiz complete! Exiting game...");
        Application.Quit();
    }

    //Delay of delay seconds
    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textWarning.text = "";
    }
}
