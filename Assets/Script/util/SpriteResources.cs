using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteResources : MonoBehaviour
{
    public static SpriteResources Instance;

    [Header("Stations Sprite")]
    public Sprite harvestStation;
    public Sprite pluckStation;
    public Sprite plantStation;
    public Sprite sampleStation;
    public Sprite sprayStation;
    public Sprite tillStation;
    public Sprite discardStation;

    private void Awake()
    {
        Instance = this;
    }
}
