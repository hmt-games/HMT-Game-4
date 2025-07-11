using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameConstant;
using UnityEngine.Serialization;

[System.Serializable]
public class ActionTimeEntry
{
    public string action;
    public float time;
}

[CreateAssetMenu(fileName = "BotMode_", menuName = "Config/BotMode")]
public class BotModeSO : ScriptableObject
{



    [Header("Bot Mode")]
    public string botModeName;

    [Header("Inventory Capacity")]
    [Range(0, 100)] public float reservoirCapacity;
    [Range(0, 100)] public int plantInventoryCapacity;

    [SerializeField]
    public List<ActionTimeEntry> supportedActions = new List<ActionTimeEntry>();
    // defined in tiles per game tick
    // e.g. game tick rate = 1 sec / tick
    //      movementSpeed = 2
    //      ==> speed = 2 tiles / sec
    [Header("Bot Capabilities")]
    public float movementSpeed;
    public Vector2Int sensingRange;
    
    // bot action speed
    //public List<ActionTimeEntry> actionTimeData = new();

    [Header("Bot Appearance")]
    public Sprite hatSprite;

    public bool ActionSupported(string action) {
        // Check if the action is supported by this bot mode
        return supportedActions.Exists(entry => entry.action == action && !float.IsNaN(entry.time));
    }

    public float GetActionTime(string action) {
        // Get the time for the specified action, or return NaN if not supported
        var entry = supportedActions.Find(e => e.action == action);
        return entry != null ? entry.time : float.NaN;
    }

    //private Dictionary<string, float> _actionTimes = new();

    //public float GetActionTime(string action)
    //{
    //    return _actionTimes.TryGetValue(action, out var value) ? value : 1.0f;
    //}

    //internal void SetActionTime(string action, float time)
    //{
    //    _actionTimes[action] = time;

    //    // Maintain serialized data for persistence
    //    var entry = actionTimeData.Find(e => e.action == action);
    //    if (entry != null)
    //        entry.time = time;
    //    else
    //        actionTimeData.Add(new ActionTimeEntry { action = action, time = time });
    //}

    //internal void RemoveActionTime(string action)
    //{
    //    _actionTimes.Remove(action);

    //    for (int i = 0; i < actionTimeData.Count; i++)
    //    {
    //        if (actionTimeData[i].action == action)
    //        {
    //            actionTimeData.RemoveAt(i);
    //            break;
    //        }
    //    }
    //}

    //internal void ClearActionTimes()
    //{
    //    _actionTimes.Clear();
    //}
}

#if UNITY_EDITOR

// why is it so convoluted to implement custom editor actions with persistant saved states???
// no idea whether this is the correct way to do things, but it works

[CustomEditor(typeof(BotModeSO))]
public class OptionSelectorEditor : Editor
{


    private BotModeSO botModeSO;

    void OnEnable()
    {
        botModeSO = (BotModeSO)target;

        if(botModeSO.supportedActions.Count < GLOBAL_CONSTANTS.ACTION_NAMES.Length) {
            // Initialize supportedActions with all actions if not already set
            foreach (var action in GLOBAL_CONSTANTS.ACTION_NAMES) {
                botModeSO.supportedActions.Add(new ActionTimeEntry() { action = action, time = float.NaN });
            }
        }

        //foreach (var entry in botModeSO.actionTimeData)
        //{
        //    botModeSO.SetActionTime(entry.action, entry.time);
        //}
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector except supportedActions
        DrawPropertiesExcluding(serializedObject, "supportedActions");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Supported Actions & Times", EditorStyles.boldLabel);

        for (int i = 0; i < botModeSO.supportedActions.Count; i++)
        {
            string action = botModeSO.supportedActions[i].action;
            bool isEnabled = botModeSO.supportedActions[i].time != float.NaN;

            EditorGUILayout.BeginHorizontal();

            bool newIsEnabled = EditorGUILayout.ToggleLeft(action, isEnabled, GUILayout.Width(120));

            if (newIsEnabled != isEnabled)
            {
                Undo.RecordObject(botModeSO, "Toggle Action");

                if (newIsEnabled)
                {
                    botModeSO.supportedActions[i].time = 1.0f; // default time
                }
                else
                {
                    botModeSO.supportedActions[i].time = float.NaN; // disable action
                }
            }

            if (newIsEnabled)
            {
                float time = botModeSO.supportedActions[i].time;
                float newTime = EditorGUILayout.FloatField(time, GUILayout.Width(60));
                if (!Mathf.Approximately(newTime, time))
                    botModeSO.supportedActions[i].time = newTime;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed) {
            //ApplyTimeToSO();
            EditorUtility.SetDirty(botModeSO);
        }

        serializedObject.ApplyModifiedProperties();
    }

    //private void ApplyTimeToSO()
    //{
    //    botModeSO.ClearActionTimes();
    //    foreach (var action in botModeSO.supportedActions)
    //    {
    //        float time = 1.0f;

    //        var entry = botModeSO.actionTimeData.Find(e => e.action == action);
    //        if (entry != null)
    //            time = entry.time;

    //        botModeSO.SetActionTime(action, time);
    //    }
    //}
}

#endif
