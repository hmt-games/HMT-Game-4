using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMathTestManager : MonoBehaviour
{
    private TestConfig _testConfig;

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
                if (!plant.enable) continue;
                GameObject nPlantObj = new GameObject($"Plant{plantCount}");
                PlantBehaviorLocalTest nPlant = nPlantObj.AddComponent<PlantBehaviorLocalTest>();
                nPlant.config = plant.plantConfig;
                nPlant.SetInitialProperties(plant.plantInitInfo);
                
                nGrid.plants.Add(nPlant);
                nPlantObj.transform.SetParent(nSoilObj.transform);
            }

            plantCount = 0;
        }
    }
}
