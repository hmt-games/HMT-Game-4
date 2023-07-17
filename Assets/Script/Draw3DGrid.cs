using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using util.GridRepresentation;

public class Draw3DGrid : MonoBehaviour
{
    [SerializeField] private Grid3DTheme grid3DTheme;
    public LevelConfig levelConfig;

    public void Create3DGrid()
    {
        transform.position = Vector3.zero;
        
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        for (int layer = 0; layer < levelConfig.layerCount; layer++)
        {
            for (int i = 0; i < levelConfig.width; i++)
            {
                for (int j = 0; j < levelConfig.height; j++)
                {
                    GameObject objToSpawn = grid3DTheme.normalGrid[Random.Range(0, grid3DTheme.normalGrid.Count)];
                
                    Transform nTile = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity).transform;
                    nTile.parent = transform;
                    nTile.localPosition = GridRepresentation.PositionFromGridCoord(i, j, layer);
                    nTile.localRotation = Quaternion.Euler(new Vector3(-90.0f, 90.0f * Random.Range(0, 4), 0.0f));
                    nTile.localScale = new Vector3(grid3DTheme.scaleBias, grid3DTheme.scaleBias, grid3DTheme.scaleBias);
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Draw3DGrid))]
public class Draw3DGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var drawGrid = (Draw3DGrid)target;
        EditorGUI.BeginChangeCheck();
        
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() || GUILayout.Button("Recreate"))
        {
            drawGrid.Create3DGrid();
        }
    }
}
#endif
