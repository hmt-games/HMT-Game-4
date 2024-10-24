using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tower parentTower;

    public bool readyToProceed = false;

    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    public void Tick()
    {
        if (BasicSpawner._runner.IsServer)
        {
            parentTower.OnTick();
        }
        //HeatMapSwicher.S.SwitchOnHeatMap();
    }
    public void Update()
    {


    }
}
