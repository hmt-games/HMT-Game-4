using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics.Contracts;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StationConfig", menuName = "Config/Station", order = 1)]
public class StationConfigSO : ScriptableObject {

    public enum StationInteraction {
        Score,
        Trash,
        SwitchBotMode,
        Reservoir,
        SeedBank
    }

    public StationInteraction interaction;

    public float interactionTime = 1f;

    [HideInInspector]
    public List<BotModeSO> botModes;

    [HideInInspector]
    public NutrientSolution reservoirAddition = NutrientSolution.Empty;

    [HideInInspector]
    public PlantConfigSO seedConfig;
}


#if UNITY_EDITOR
[CustomEditor(typeof(StationConfigSO))]
public class StationConfigSOEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        StationConfigSO config = (StationConfigSO)target;

        switch (config.interaction) {
            case StationConfigSO.StationInteraction.Score:
            case StationConfigSO.StationInteraction.Trash:
                break;
            case StationConfigSO.StationInteraction.SwitchBotMode:
                SerializedObject serializedObject = new SerializedObject(config);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("botModes"));

                break;
            case StationConfigSO.StationInteraction.Reservoir:
                Vector4 nutirients = config.reservoirAddition.nutrients;
                nutirients.x = EditorGUILayout.Slider("A", nutirients.x, 0, 1.0f);
                nutirients.y = EditorGUILayout.Slider("B", nutirients.y, 0, 1.0f);
                nutirients.z = EditorGUILayout.Slider("C", nutirients.z, 0, 1.0f);
                nutirients.w = EditorGUILayout.Slider("D", nutirients.w, 0, 1.0f);
                config.reservoirAddition.nutrients = nutirients;
                config.reservoirAddition.water = 1.0f;


                break;
            case StationConfigSO.StationInteraction.SeedBank:
                config.seedConfig = (PlantConfigSO)EditorGUILayout.ObjectField("Seed Config", config.seedConfig, typeof(PlantConfigSO), false);
                break;
        }
    }
}

#endif