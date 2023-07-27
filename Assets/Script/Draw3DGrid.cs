using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using util.GridRepresentation;




public class Draw3DGrid : MonoBehaviour
{
    [SerializeField] private Grid3DTheme grid3DTheme;
    public LevelConfig levelConfig;


    GridLayer gridLayer;


<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    private void Start()
    {
        Create3DGrid();
    }

<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    public void Create3DGrid()
    {
        transform.position = Vector3.zero;
        
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        

        for (int layer = 0; layer < levelConfig.layerCount; layer++)
        {
            gridLayer = new GridLayer(levelConfig.width,levelConfig.height);
            

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
                    
                    // Add collider & layer for mouse position to world ray cast
                    GameObject nObj = nTile.gameObject;
                    nObj.AddComponent<BoxCollider>();
                    nObj.layer = GridRepresentation.gridLayer;
                    // Add GridInfo MonoBehavior for grids to store their own states
                    GridNode nGirdNodeInfo = nObj.AddComponent<GridNode>();
                    nGirdNodeInfo.coordinate = new Vector2Int(i, j);
                    nGirdNodeInfo.layer = layer;

                    gridLayer.nodesOnThisGridLayer[i, j] = nGirdNodeInfo;
                }
            }

            //Store all layers to create a world map
            GameConstants.GameMap.allGridLayers.Add(gridLayer);
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
