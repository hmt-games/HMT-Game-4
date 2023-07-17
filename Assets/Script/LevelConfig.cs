using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Config/Level")]
public class LevelConfig : ScriptableObject
{
    public int width;
    public int height;
    public int layerCount;
}
