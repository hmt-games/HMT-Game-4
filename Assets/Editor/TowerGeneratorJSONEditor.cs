using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerGeneratorJSON))]
public class TowerGeneratorJSONEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TowerGeneratorJSON generator = (TowerGeneratorJSON)target;
        
        if (GUILayout.Button("Generate"))
        {
            // Call the function when the button is pressed
            generator.Generate();
        }
    }
}
