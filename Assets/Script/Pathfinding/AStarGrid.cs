using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AStarGrid : MonoBehaviour
{
    public static AStarGrid Instance;
    
    [Header("Visualization")]
    [SerializeField] private bool ShowGrid = false;
    [SerializeField] private bool ShowPath = false;
    
    public LayerMask UnwalkableLayerMask;
    public Vector2 GridSize;
    [Range(0.15f, 5.0f)] public float NodeRadius;
    private AStarNode[,] _grid;

    private Vector3 _lowerLeftCorner;
    private float _nodeDiameter;
    private int _gridCountX, _gridCountY;

    [HideInInspector] public int GridMaxSize;
    [HideInInspector] public List<AStarNode> path;

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;
    }

    public AStarNode WorldPointToNode(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - _lowerLeftCorner.x) / GridSize.x;
        float percentY = (worldPosition.y - _lowerLeftCorner.y) / GridSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int xIdx = (int)((_gridCountX-1) * percentX);
        int yIdx = (int)((_gridCountY-1) * percentY);
        return _grid[xIdx, yIdx];
    }

    public List<AStarNode> GetNeighbours(AStarNode node)
    {
        List<AStarNode> ret = new List<AStarNode>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;
                int neighbourX = node.GridX + i;
                int neighbourY = node.GridY + j;
                if (neighbourX >= 0 && neighbourX < _gridCountX && neighbourY >=0 && neighbourY < _gridCountY)
                    ret.Add(_grid[neighbourX, neighbourY]);
            }
        }

        return ret;
    }

    private void OnValidate()
    {
        _lowerLeftCorner = transform.position - new Vector3(GridSize.x, GridSize.y, 0.0f) / 2;
        _nodeDiameter = NodeRadius * 2.0f;
        _gridCountX = Mathf.RoundToInt(GridSize.x / _nodeDiameter);
        _gridCountY = Mathf.RoundToInt(GridSize.y / _nodeDiameter);
        GridMaxSize = _gridCountX * _gridCountY;
        
        CreateGrid();
    }

    public void CreateGrid()
    {
        _grid = new AStarNode[_gridCountX, _gridCountY];
        
        for (int x = 0; x < _gridCountX; x++)
        {
            for (int y = 0; y < _gridCountY; y++)
            {
                Vector3 worldPoint = _lowerLeftCorner
                                     + Vector3.right * (x * _nodeDiameter + NodeRadius)
                                     + Vector3.up * (y * _nodeDiameter + NodeRadius);
                bool walkable = !Physics2D.CircleCast(
                    worldPoint, NodeRadius, 
                    Vector3.forward, 1.0f, UnwalkableLayerMask);
                _grid[x, y] = new AStarNode(worldPoint, walkable, x, y);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowGrid)
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, GridSize.y, 1));

            if (_grid != null)
            {
                foreach (AStarNode node in _grid)
                {
                    Gizmos.color = node.Walkable ? Color.green : Color.red;
                    Gizmos.DrawCube(node.NodePosition, Vector3.one * (_nodeDiameter - 0.1f));
                }
            }
        }

        if (ShowPath && path != null && path.Count != 0)
        {
            Gizmos.color = Color.black;
            foreach (AStarNode waypoint in path)
            {
                Gizmos.DrawCube(waypoint.NodePosition, Vector3.one * (_nodeDiameter - 0.1f));
            }
        }
    }
}


[CustomEditor(typeof(AStarGrid))]
public class AStarGridEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        AStarGrid aStarGrid = (AStarGrid)target;

        if (GUILayout.Button("Regenerate")) {
            aStarGrid.CreateGrid();
            SceneView.RepaintAll();
        }
    }
}
