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
    
    
    public struct GLOBAL_CONSTANTS
    {
        public const int MAX_PLANT_COUNT_PER_TILE = 9;
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