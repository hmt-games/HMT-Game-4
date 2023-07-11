using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Config/Level")]
public class LevelConfig : ScriptableObject
{
    public int width;
    public int height;
    public Vector2 playerStartPos;
    public Vector2 goalPos;
    public List<Vector2> keysPos;
    public List<Vector2> reversesPos;
    public List<Vector2> blocksPos;
    public int steps;
}
