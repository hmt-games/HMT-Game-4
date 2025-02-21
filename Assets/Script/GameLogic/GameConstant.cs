using System.Collections.Generic;

namespace GameConstant
{
    public enum NutrientType
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3
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
}