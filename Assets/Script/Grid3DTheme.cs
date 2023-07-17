using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Theme/3D Board")]
public class Grid3DTheme : ScriptableObject
{
    public float scaleBias = 0.5f;
    
    public List<GameObject> normalGrid;
    public List<GameObject> stoneGrid;
}
