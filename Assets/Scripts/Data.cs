using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Data")]
public class Data : ScriptableObject
{
    [SerializeField]
    private List<Level> levels;
    public int currentLevelIndex;

    public List<Level> Levels
    {
        get
        {
            if (levels == null)
                levels = new List<Level>();
            return levels;
        }
        set { levels = value; }
    }
}

[System.Serializable]
public class Level
{
    public Sprite questionSprite;
    public Sprite wrongAnswerSprite;
    public int wordCount;
    public List<string> words = new List<string>();
}