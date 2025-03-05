using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    // Displays the word on the button.
    public Text wordText;

    // Shows the selection order for this button.
    public Text indexText;

    // Keeps track of the button's selection order (0 means not yet selected).
    public int currentIndex;

    // Stores the original word for this button.
    private string originalWord;

    // Reference to the manager that handles option buttons.
    private OptionsPlacing manager;

    private void Awake()
    {
        // If the wordText hasn't been set in the Inspector, try to auto-assign it.
        if (wordText == null)
        {
            wordText = GetComponentInChildren<Text>();
            if (wordText == null)
                Debug.LogWarning("Text component not found on OptionButton prefab.");
        }
    }

    // Sets up the button with the given word and a reference to its manager.
    public void Initialize(OptionsPlacing optionsManager, string word)
    {
        manager = optionsManager;
        originalWord = word;
        if (wordText != null)
            wordText.text = word;
        currentIndex = 0;
    }

    // Triggered when the button is clicked.
    public void OnButtonClicked()
    {
        Debug.Log("Button clicked: " + originalWord);
        if (manager != null)
            manager.HandleOptionButtonClick(this);
    }

    // Updates the button's selection order and displays it in the UI.
    public void SetIndex(int index)
    {
        currentIndex = index;
        if (wordText != null)
        {
            // Display the new index on the button.
            indexText.text = currentIndex.ToString();
        }
        else
        {
            Debug.LogWarning("wordText is not assigned.");
        }
    }
}
