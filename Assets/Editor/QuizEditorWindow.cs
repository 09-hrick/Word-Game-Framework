using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuizEditorWindow : EditorWindow
{
    private Data dataAsset; // Reference to ScriptableObject
    private Vector2 scrollPos;
    private const string dataAssetPath = "Assets/ScriptableObjects/Data.asset";

    [MenuItem("Window/Quiz Editor")]
    public static void ShowWindow()
    {
        GetWindow<QuizEditorWindow>("Quiz Editor");
    }

    private void OnEnable()
    {
        const string folderPath = "Assets/ScriptableObjects";
        const string dataAssetPath = folderPath + "/Data.asset";

        // Check if the folder exists. If not, create it.
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            Debug.Log("Created folder: " + folderPath);
        }

        // Try to load the asset from the specified path.
        dataAsset = AssetDatabase.LoadAssetAtPath<Data>(dataAssetPath);

        // If not found, create a new asset.
        if (dataAsset == null)
        {
            dataAsset = ScriptableObject.CreateInstance<Data>();
            AssetDatabase.CreateAsset(dataAsset, dataAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created new Data asset at " + dataAssetPath);
        }
    }


    private void OnGUI()
    {
        GUILayout.Label("Quiz Editor", EditorStyles.boldLabel);

        if (dataAsset == null)
        {
            EditorGUILayout.HelpBox("Data asset not found or not loaded.", MessageType.Warning);
            return;
        }

        // Access the levels list from your Data asset.
        List<Level> levels = dataAsset.Levels; // Make sure Levels is public or has a public property

        // Input for the number of levels.
        int currentLevelCount = levels.Count;
        int newLevelCount = EditorGUILayout.IntField("Number of Levels", currentLevelCount);
        if (newLevelCount != currentLevelCount)
        {
            if (newLevelCount > currentLevelCount)
            {
                while (newLevelCount > levels.Count)
                {
                    levels.Add(new Level());
                }
            }
            else
            {
                while (newLevelCount < levels.Count)
                {
                    levels.RemoveAt(levels.Count - 1);
                }
            }
            EditorUtility.SetDirty(dataAsset);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < levels.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Level " + (i + 1), EditorStyles.boldLabel);

            Level currentLevel = levels[i];

            // Object field for the question sprite.
            currentLevel.questionSprite = (Sprite)EditorGUILayout.ObjectField("Question Sprite:", currentLevel.questionSprite, typeof(Sprite), false);
            currentLevel.wrongAnswerSprite = (Sprite)EditorGUILayout.ObjectField("Wrong Answer Sprite:", currentLevel.wrongAnswerSprite, typeof(Sprite), false);

            // Input for the number of words for this level.
            int newWordCount = EditorGUILayout.IntField("Number of Words", currentLevel.wordCount);
            if (newWordCount > 5)
            {
                EditorUtility.DisplayDialog("Warning", "Can't have more than 5 words in a level", "OK");
                Debug.LogWarning("Warning: Can't have more than 5 words in a level. Setting count to 5.");
                newWordCount = 5;
            }

            if (newWordCount != currentLevel.wordCount)
            {
                currentLevel.wordCount = newWordCount;
                // Adjust the words list to match the word count.
                if (newWordCount > currentLevel.words.Count)
                {
                    while (newWordCount > currentLevel.words.Count)
                    {
                        currentLevel.words.Add("");
                    }
                }
                else
                {
                    while (newWordCount < currentLevel.words.Count)
                    {
                        currentLevel.words.RemoveAt(currentLevel.words.Count - 1);
                    }
                }
                EditorUtility.SetDirty(dataAsset);
            }

            // Display text fields for each word.
            for (int j = 0; j < currentLevel.words.Count; j++)
            {
                currentLevel.words[j] = EditorGUILayout.TextField("Word " + (j + 1), currentLevel.words[j]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dataAsset);
        }
    }
}
