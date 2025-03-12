using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    public Text wordText;
    public int currentIndex;
    private string originalWord;
    private GameController manager;

    private void Awake()
    {
        if (wordText == null)
        {
            wordText = GetComponentInChildren<Text>();
            if (wordText == null)
                Debug.LogWarning("Text component not found on OptionButton prefab.");
        }
    }

    public void Initialize(GameController optionsManager, string word)
    {
        manager = optionsManager;
        originalWord = word;
        if (wordText != null)
            wordText.text = word;
        currentIndex = 0;
    }

    public void OnButtonClicked()
    {
        Debug.Log("Button clicked: " + originalWord);
        if (manager != null)
            manager.HandleOptionButtonClick(this);
    }

    public void SetIndex(int index)
    {
        currentIndex = index;
    }
}
