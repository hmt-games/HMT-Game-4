using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConstant
{

    public enum NutrientType
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3
    }

    public enum TileType
    {
        Soil,
        HarvestStation,
        PluckStation,
        PlantStation,
        SampleStation,
        SprayAStation,
        SprayBStation,
        SprayCStation,
        SprayDStation,
        TillStation,
        CarryStation,
        DiscardStation,
        ScoreStation
    }
    
    public enum BotMode {
        Normal,
        Harvest,
        Pluck,
        Till,
        Spray,
        Sample,
        Plant,
        Pick,
        Carry
    }

    public enum InventoryMode {
        None,
        Reservoir,
        PlantInventory,
        Hybrid
    }


    public struct GLOBAL_CONSTANTS
    {
        public const int MAX_PLANT_COUNT_PER_TILE = 9;
      
        public static readonly string[] ACTION_NAMES = new[] {
            "move", "move_to", "harvest", "sample",
            "spray", "pick", "take", "drop",
            "plant", "pluck", "till", "interact"
        };
    }

    public struct GLOBAL_FUNCTIONS
    {
        public int GetRandomFalseIndex(bool[] bools)
        {
            List<int> falseIndices = new List<int>();

            for (int i = 0; i < bools.Length; i++)
            {
                if (!bools[i])
                {
                    falseIndices.Add(i);
                }
            }

            if (falseIndices.Count == 0)
            {
                return -1;
            }

            int randomIndex = UnityEngine.Random.Range(0, falseIndices.Count);
            return falseIndices[randomIndex];
        }

        public static float GausianRandom(float mean, float stdDev, int sigmaClamp = 3) {
            // Box-Muller transform to generate a Gaussian random number
            float u1 = UnityEngine.Random.value;
            float u2 = UnityEngine.Random.value;
            float z0 = Mathf.Clamp(Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2), -sigmaClamp, sigmaClamp);
            return z0 * stdDev + mean;
        }
    }

    [Serializable]
    public struct PlantInitInfo
    {
        public float RootMass;
        public float Height;
        public float EnergyLevel;
        public float Health;
        public float Age;
        public float Water;
        public Vector4 Nutrient;

        public PlantInitInfo(float rootMass, float height, float energyLevel, float health, float age, float water, Vector4 nutrient)
        {
            RootMass = rootMass;
            Height = height;
            EnergyLevel = energyLevel;
            Health = health;
            Age = age;
            Water = water;
            Nutrient = nutrient;
        }
    }
}