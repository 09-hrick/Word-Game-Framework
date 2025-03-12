using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuizEditorWindow : EditorWindow
{
    private Data dataAsset; // Our ScriptableObject that holds quiz data
    private Vector2 scrollPos; // For scrolling through levels in the window
    private const string dataAssetPath = "Assets/Resources/Data.asset";

    [MenuItem("Window/Quiz Editor")]
    public static void ShowWindow()
    {
        // Open the Quiz Editor window
        GetWindow<QuizEditorWindow>("Quiz Editor");
    }

    private void OnEnable()
    {
        const string folderPath = "Assets/Resources";
        const string dataAssetPath = folderPath + "/Data.asset";

        // Check if the folder exists, and create it if needed.
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            Debug.Log("Created folder: " + folderPath);
        }

        // Try to load the Data asset from the folder.
        dataAsset = AssetDatabase.LoadAssetAtPath<Data>(dataAssetPath);

        // If it doesn't exist, create a new Data asset.
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

        // If the asset isn't loaded, show a warning.
        if (dataAsset == null)
        {
            EditorGUILayout.HelpBox("Data asset not found or not loaded.", MessageType.Warning);
            return;
        }

        // Get the list of levels from the Data asset.
        List<Level> levels = dataAsset.Levels;

        // Let the user set how many levels there should be.
        int currentLevelCount = levels.Count;
        int newLevelCount = EditorGUILayout.IntField("Number of Levels", currentLevelCount);
        if (newLevelCount != currentLevelCount)
        {
            if (newLevelCount > currentLevelCount)
            {
                // Add new level entries as needed.
                while (newLevelCount > levels.Count)
                {
                    levels.Add(new Level());
                }
            }
            else
            {
                // Remove levels until the count matches.
                while (newLevelCount < levels.Count)
                {
                    levels.RemoveAt(levels.Count - 1);
                }
            }
            EditorUtility.SetDirty(dataAsset); // Mark asset as changed.
        }

        // Create a scrollable area for level details.
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < levels.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            // Create a horizontal layout for the header and delete button.
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Level " + (i + 1), EditorStyles.boldLabel);
            if (GUILayout.Button("Delete Level", GUILayout.Width(100)))
            {
                // Confirm deletion.
                if (EditorUtility.DisplayDialog("Delete Level", "Are you sure you want to delete Level " + (i + 1) + "?", "Yes", "No"))
                {
                    levels.RemoveAt(i);
                    EditorUtility.SetDirty(dataAsset);
                    // Exit the loop to avoid errors due to the modified list.
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
            Level currentLevel = levels[i];

            // Fields to set the sprites for this level.
            currentLevel.questionSprite = (Sprite)EditorGUILayout.ObjectField("Question Sprite:", currentLevel.questionSprite, typeof(Sprite), false);
            currentLevel.wrongAnswerSprite = (Sprite)EditorGUILayout.ObjectField("Wrong Answer Sprite:", currentLevel.wrongAnswerSprite, typeof(Sprite), false);

            // Let the user define how many words are in this level.
            int newWordCount = EditorGUILayout.IntField("Number of Words", currentLevel.wordCount);
            if (newWordCount > 12)
            {
                EditorUtility.DisplayDialog("Warning", "Can't have more than 12 words in a level", "OK");
                Debug.LogWarning("Warning: Can't have more than 12 words in a level. Setting count to 12.");
                newWordCount = 12;
            }

            if (newWordCount != currentLevel.wordCount)
            {
                currentLevel.wordCount = newWordCount;
                // Adjust the words list to match the new count.
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

            // Create text fields for each word.
            for (int j = 0; j < currentLevel.words.Count; j++)
            {
                currentLevel.words[j] = EditorGUILayout.TextField("Word " + (j + 1), currentLevel.words[j]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();

        // Mark the asset dirty if any changes occurred.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(dataAsset);
        }
    }
}
