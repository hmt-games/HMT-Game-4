using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameConstant;

[CreateAssetMenu(fileName = "BotMode_", menuName = "Bot/BotMode")]
public class BotModeSO : ScriptableObject
{
    [Header("Bot Mode")]
    public BotMode botMode;
    public string botModeName;

    [Header("Inventory Capacity")]
    [Range(0, 100)] public float reservoirCapacity;
    [Range(0, 100)] public int plantInventoryCapacity;

    public List<string> supportedActions = new List<string>();
    // defined in tiles per game tick
    // e.g. game tick rate = 1 sec / tick
    //      movementSpeed = 2
    //      ==> speed = 2 tiles / sec
    [Header("Bot Capabilities")]
    public float movementSpeed;
    public Vector2 sensingRange;


}

#if UNITY_EDITOR

// why is it so convoluted to implement custom editor actions with persistant saved states???
// no idea whether this is the correct way to do things, but it works
[CustomEditor(typeof(BotModeSO))]
public class OptionSelectorEditor : Editor
{
    private static readonly string[] allOptions = new[]
    {
        "move", "moveto", "harvest", "sample",
        "spray", "pick", "pickUp", "putDown",
        "plant", "pluck", "till", "useStation"
    };

    SerializedProperty supportedActionsProp;

    void OnEnable()
    {
        supportedActionsProp = serializedObject.FindProperty("supportedActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "supportedActions");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Supported Actions:", EditorStyles.boldLabel);

        for (int i = 0; i < allOptions.Length; i++)
        {
            string option = allOptions[i];
            bool contains = false;
            for (int j = 0; j < supportedActionsProp.arraySize; j++)
            {
                if (supportedActionsProp.GetArrayElementAtIndex(j).stringValue == option)
                {
                    contains = true;
                    break;
                }
            }

            bool newContains = EditorGUILayout.ToggleLeft(option, contains);

            if (newContains != contains)
            {
                // Record an undo so changes persist in the Undo history
                Undo.RecordObject(target, "Toggle Supported Action");

                if (newContains)
                {
                    supportedActionsProp.arraySize++;
                    supportedActionsProp
                        .GetArrayElementAtIndex(supportedActionsProp.arraySize - 1)
                        .stringValue = option;
                }
                else
                {
                    for (int j = 0; j < supportedActionsProp.arraySize; j++)
                    {
                        if (supportedActionsProp.GetArrayElementAtIndex(j).stringValue == option)
                        {
                            supportedActionsProp.DeleteArrayElementAtIndex(j);
                            break;
                        }
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
