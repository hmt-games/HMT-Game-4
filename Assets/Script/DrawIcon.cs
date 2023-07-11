using UnityEditor;
using UnityEngine;

public class DrawIcon : MonoBehaviour
{
    public LevelConfig levelConfig;
    public IconTheme iconTheme;

    public void CreateGraphicalIcon()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        CreateIcon(levelConfig.playerStartPos, iconTheme.playerIcon);
        CreateIcon(levelConfig.goalPos, iconTheme.goalIcon);

        foreach (Vector2 keyPos in levelConfig.keysPos)
        {
            CreateIcon(keyPos, iconTheme.keyIcon);
        }

        foreach (Vector2 reversePos in levelConfig.reversesPos)
        {
            CreateIcon(reversePos, iconTheme.reverseIcon);
        }

        foreach (Vector2 blockPos in levelConfig.blocksPos)
        {
            CreateIcon(blockPos, iconTheme.blockIcon);
        }
    }

    private void CreateIcon(Vector2 pos, Texture2D tex)
    {
        Shader gridShader = Shader.Find ("Universal Render Pipeline/Lit");
        
        Transform nQuad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        Material gridMat = new Material(gridShader);
        gridMat.SetTexture("_BaseMap", tex);
        nQuad.GetComponent<MeshRenderer>().material = gridMat;
        nQuad.parent = transform;
        nQuad.localPosition =
            GridRepresentation.PositionFromGridCoord((int)pos.x, (int)pos.y) + Vector3.up * 0.1f;
        nQuad.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(DrawIcon))]
public class DrawIconEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var drawIcon = (DrawIcon)target;
        EditorGUI.BeginChangeCheck();
        
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() || GUILayout.Button("Recreate"))
        {
            drawIcon.CreateGraphicalIcon();
        }
    }
}
#endif
