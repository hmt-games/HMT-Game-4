using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteResources : MonoBehaviour
{
    public static SpriteResources Instance;

    [Header("Stations Sprite")]
    public Sprite discardStation;
    public Sprite scoreStation;
    public Sprite modeChangeStation;
    public Sprite reservoirStation;
    public Sprite seedBankStation;

    //public Sprite pluckStation;
    //public Sprite plantStation;
    //public Sprite sampleStation;
    //public Sprite sprayAStation;
    //public Sprite sprayBStation;
    //public Sprite sprayCStation;
    //public Sprite sprayDStation;
    //public Sprite tillStation;


    private void Awake()
    {
        Instance = this;
    }
}
