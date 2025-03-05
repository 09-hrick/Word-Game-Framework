using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Data")]
public class Data : ScriptableObject
{
    [SerializeField]
    private List<Level> levels; // Stores all game levels

    public int currentLevelIndex; // Tracks the current level

    // Getter and setter for the levels list
    public List<Level> Levels
    {
        get
        {
            if (levels == null)
                levels = new List<Level>(); // Ensure list is initialized
            return levels;
        }
        set { levels = value; }
    }
}

[System.Serializable]
public class Level
{
    public Sprite questionSprite; // Image for the question
    public Sprite wrongAnswerSprite; // Image shown on incorrect answer
    public int wordCount; // Number of words in this level
    public List<string> words = new List<string>(); // List of words for the level
}
