using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public GridTheme gridTheme;
    public LevelConfig levelConfig;

    public void CreateGraphicalBoard(int rowCount, int columnCount)
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        Shader gridShader = Shader.Find ("Universal Render Pipeline/Lit");
        
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                bool isLightSquare = (i + j) % 2 != 0;
                Color color = isLightSquare ? gridTheme.lightColor : gridTheme.darkColor;

                Transform nQuad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                Material gridMat = new Material(gridShader);
                gridMat.color = color;
                nQuad.GetComponent<MeshRenderer>().material = gridMat;
                nQuad.parent = transform;
                nQuad.localPosition = GridRepresentation.PositionFromGridCoord(j, i);
                nQuad.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            }
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(DrawGrid))]
public class DrawGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var drawGrid = (DrawGrid)target;
        EditorGUI.BeginChangeCheck();
        
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() || GUILayout.Button("Recreate"))
        {
            drawGrid.CreateGraphicalBoard(drawGrid.levelConfig.height, drawGrid.levelConfig.width);
        }
    }
}
#endif