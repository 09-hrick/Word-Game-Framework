using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    public Text wordText;         // UI Text to display the word.
    public Text indexText;        // UI Text to display the index.
    public int currentIndex = 0;  // Order of selection.
    private string originalWord;
    private OptionsPlacing manager;
    
    private void Awake()
    {
        // Auto-assign if not set in the Inspector.
        if (wordText == null)
        {
            wordText = GetComponentInChildren<Text>();
            if (wordText == null)
                Debug.LogWarning("Text component not found on OptionButton prefab.");
        }
    }

    // Initializes the button with its word and a reference to the manager.
    public void Initialize(OptionsPlacing optionsManager, string word)
    {
        manager = optionsManager;
        originalWord = word;
        if (wordText != null)
            wordText.text = word;
        currentIndex = 0;
    }

    // Called when the button is clicked.
    public void OnButtonClicked()
    {
        Debug.Log("Button clicked: " + originalWord);
        if (manager != null)
            manager.HandleOptionButtonClick(this);
    }

    // Updates the button's index and reflects it in the UI text.
    public void SetIndex(int index)
    {
        currentIndex = index;
        if (wordText != null)
        {
            // Update the text to show the index number.
            indexText.text = currentIndex.ToString();
        }
        else
        {
            Debug.LogWarning("wordText is not assigned.");
        }
    }
}
