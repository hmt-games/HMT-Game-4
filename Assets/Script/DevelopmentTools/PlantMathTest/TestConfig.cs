using System;
using System.Collections;
using System.Collections.Generic;
using GameConstant;
using UnityEngine;

public class TestConfig : MonoBehaviour
{
    public List<PlantMathTestUnit> testSoilUnits;

    #region singleton declearation

    public static TestConfig Instance;

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;
    }

    #endregion
}

[Serializable]
public class PlantMathTestUnit
{
    public bool enable = true;
    public SoilInitConfig soilInitConfig;
    public List<PlantInitConfig> plants;
}

[Serializable]
public class SoilInitConfig
{
    public SoilConfigSO soilConfig;
    public NutrientSolution nutrientSolution;
}

[Serializable]
public class PlantInitConfig
{
    public bool enable = true;
    public PlantConfigSO plantConfig;
    public PlantInitInfo plantInitInfo;
}
