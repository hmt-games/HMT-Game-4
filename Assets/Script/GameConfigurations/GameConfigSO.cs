using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class GameGoalEntry
{
    public string goalName;
    public int goalValue;
}

[CreateAssetMenu(fileName = "Game_", menuName = "Config/Game")]
public class GameConfigSO : ScriptableObject
{
    [Header("Game Configs")]
    public float secondPerTick = 1.0f;
    public TextAsset configJSON;
    public TextAsset towerJSON;

    public List<GameGoalEntry> gameGoalEntries = new();

    public Dictionary<string, int> GetGameGoalDict()
    {
        Dictionary<string, int> dict = new();
        foreach (var entry in gameGoalEntries)
        {
            if (!string.IsNullOrEmpty(entry.goalName))
                dict[entry.goalName] = entry.goalValue;
        }
        return dict;
    }

    [Space(5)]
    [Header("Bot Mode for Stations")]
    public BotModeSO harvest;
    public BotModeSO spray;
    public BotModeSO pick;
    public BotModeSO plant;
    public BotModeSO till;
    public BotModeSO sample;
}

#if UNITY_EDITOR

[CustomEditor(typeof(GameConfigSO))]
public class GameConfigSOEditor : Editor
{
    private GameConfigSO config;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        config = (GameConfigSO)target;

        // Draw default inspector except for gameGoalEntries
        DrawPropertiesExcluding(serializedObject, "gameGoalEntries");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Game Goal Entries", EditorStyles.boldLabel);

        if (config.gameGoalEntries.Count == 0)
            EditorGUILayout.LabelField("No goals set.");

        for (int i = 0; i < config.gameGoalEntries.Count; i++)
        {
            var entry = config.gameGoalEntries[i];
            EditorGUILayout.BeginHorizontal();
            entry.goalName = EditorGUILayout.TextField(entry.goalName);
            entry.goalValue = EditorGUILayout.IntField(entry.goalValue);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                config.gameGoalEntries.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Add Goal"))
        {
            config.gameGoalEntries.Add(new GameGoalEntry { goalName = "Plant", goalValue = 10 });
        }

        if (GUI.changed)
            EditorUtility.SetDirty(config);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif