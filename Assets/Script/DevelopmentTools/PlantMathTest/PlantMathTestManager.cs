using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMathTestManager : MonoBehaviour
{
    private TestConfig _testConfig;

    private List<GridBehaviorLocalTest> _simulatedGrid;

    private void Awake()
    {
        _simulatedGrid = new List<GridBehaviorLocalTest>();
    }

    private void Start()
    {
        _testConfig = TestConfig.Instance;

        int soilCount = 0;
        int plantCount = 0;
        foreach (PlantMathTestUnit soil in _testConfig.testSoilUnits)
        {
            if (!soil.enable) continue;
            
            // create soil object and init grid behavior properties
            SoilInitConfig soilInitConfig = soil.soilInitConfig;
            GameObject nSoilObj = new GameObject($"Soil{soilCount}");
            GridBehaviorLocalTest nGrid = nSoilObj.AddComponent<GridBehaviorLocalTest>();
            nGrid.soilConfig = soilInitConfig.soilConfig;
            nGrid.NutrientLevels = soilInitConfig.nutrientSolution;
            nGrid.plants = new List<PlantBehaviorLocalTest>();

            foreach (PlantInitConfig plant in soil.plants)
            {
                CreatePlant(plant, plantCount, nGrid);
            }

            plantCount = 0;
            _simulatedGrid.Add(nGrid);
        }
    }

    private void CreatePlant(PlantInitConfig plant, int plantCount, GridBehaviorLocalTest parentSoil)
    {
        if (!plant.enable) return;
        GameObject nPlantObj = new GameObject($"Plant{plantCount}");
        PlantBehaviorLocalTest nPlant = nPlantObj.AddComponent<PlantBehaviorLocalTest>();
        nPlant.config = plant.plantConfig;
        nPlant.SetInitialProperties(plant.plantInitInfo);
                
        parentSoil.plants.Add(nPlant);
        nPlantObj.transform.SetParent(parentSoil.transform);
        
        PlantMathDataLogger.Instance.AddPlantToTrackList(nPlant);
    }

    public void OnTick()
    {
        foreach (GridBehaviorLocalTest grid in _simulatedGrid)
        {
            grid.OnTick();
        }
        
        PlantMathDataLogger.Instance.OnTick();
    }

    public void Retract()
    {
        PlantMathDataLogger.Instance.Retract();
    }
}
