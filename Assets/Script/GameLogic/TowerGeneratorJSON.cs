using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class TowerGeneratorJSON : MonoBehaviour
{
    [Header("Tower Dimension")]
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private int depth;

    [Header("Generator Parameter")]
    [SerializeField] private float intensity;
    [SerializeField] private float variance;

    [Header("File Settings")] 
    [SerializeField] private string levelName;

    private Dictionary<string, object> _configs;
    private Dictionary<string, object> _tower;
    private List<string> _plantConfigNames;
    private List<string> _soilConfigNames;

    private int _numOfPlantsConfigs;
    private int _numOfSoilConfigs;

    public void Generate()
    {
        _configs = new Dictionary<string, object>();
        _tower = new Dictionary<string, object>();
        _plantConfigNames = new List<string>();
        _soilConfigNames = new List<string>();

        _numOfPlantsConfigs = Random.Range(1, 4);
        _numOfSoilConfigs = Random.Range(1, (depth * width) / 4);
        
        // placeholder configs for now, need to generate them procedurally
        for (; _plantConfigNames.Count < _numOfPlantsConfigs;)
        {
            string name = $"plantConfig{_plantConfigNames.Count}";
            _plantConfigNames.Add(name);
            _configs[name] = new Dictionary<string, object>
            {
                { "capacities", RandomListOfFour(5.0f, 10.0f) },
                { "uptakeRate", RandomListOfFour(0.3f, 1.0f) },
                { "metabolismNeeds", RandomListOfFour(0.5f, 1.5f) },
                { "metabolismFactor", RandomListOfFour(0.0f, 1.0f) },
                { "growthConsumptionRateLimit", RandomListOfFour(1.0f, 3.0f) },
                { "growthFactor", RandomListOfFour(-2.0f, 8.0f) },
                { "rootHeightTransition", Random.Range(2.0f, 8.0f) },
                { "growthToleranceThreshold", Random.Range(1.0f, 2.5f) }
            };
        }

        for (; _soilConfigNames.Count < _numOfSoilConfigs;)
        {
            string name = $"_soilConfig{_soilConfigNames.Count}";
            _soilConfigNames.Add(name);
            _configs[name] = new Dictionary<string, object>
            {
                { "capacities", new Dictionary<string, object> 
                    {
                        { "water", Random.Range(0.0f, 10.0f) },
                        { "nutrients", RandomListOfFour(4.0f, 10.0f) }
                    }
                },
                { "drainTime", Random.Range(2.0f, 8.0f) }
            };
        }
        

        _tower["Tower"] = new Dictionary<string, int>()
        {
            {"width", width},
            {"depth", depth},
            {"height", height}
        };

        for (int level = 0; level < height; level++)
        {
            List<Dictionary<string, object>> grids = new List<Dictionary<string, object>>();
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Dictionary<string, object> NutrientLevels = new Dictionary<string, object>()
                    {
                        {"water", Random.Range(2.0f, 10.0f)},
                        {"nutrients", RandomListOfFour(1.0f, 6.0f)}
                    };

                    Dictionary<string, object> surfacePlants = new Dictionary<string, object>();
                    int numberOfPlants = Random.Range(0, 5);
                    for (int i = 0; i < numberOfPlants; i++)
                    {
                        string name = $"Plant{i}";
                        surfacePlants[name] = new Dictionary<string, object>() {
                            {"config", _plantConfigNames[Random.Range(0, _plantConfigNames.Count)]},
                            {"rootMass", 3.0f},
                            {"height", 2.0f},
                            {"energyLevel", 10.0f},
                            {"health", 2.0f},
                            {"age", 5.0f},
                            {"nutrient", new List<float>(){2.0f, 3.0f, 2.0f, 4.0f}},
                            {"water", 3.0f}
                        };
                    }

                    Dictionary<string, object> grid = new Dictionary<string, object>()
                    {
                        {"soilConfig", _soilConfigNames[Random.Range(0, _soilConfigNames.Count)]},
                        {"NutrientLevels", NutrientLevels},
                        {"surfacePlants", surfacePlants}
                    };
                    
                    grids.Add(grid);
                }
            }

            _tower[$"Floor{level}"] = grids;
        }
        
        string towerJSON = JsonConvert.SerializeObject(_tower, Formatting.Indented);
        string configJSON = JsonConvert.SerializeObject(_configs, Formatting.Indented);
        string path = Application.dataPath + "/GeneratedLevel/";
        SaveStringToFile(path + levelName + "Config" + ".json", configJSON);
        SaveStringToFile(path + levelName + "Tower" + ".json", towerJSON);
    }
    
    private void SaveStringToFile(string filePath, string text)
    {
        // Write the string to the file at the specified path
        File.WriteAllText(filePath, text);

        // Optionally, you can force Unity to refresh the Asset database
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    private List<float> RandomListOfFour(float min, float max)
    {
        return new List<float>()
        {
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max),
        };
    }
}
