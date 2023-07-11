using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Theme/Icon")]
public class IconTheme : ScriptableObject
{
    public float  iconScale;
    public Texture2D playerIcon;
    public Texture2D keyIcon;
    public Texture2D reverseIcon;
    public Texture2D goalIcon;
    public Texture2D blockIcon;
}
