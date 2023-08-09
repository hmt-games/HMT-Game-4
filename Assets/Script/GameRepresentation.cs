using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace util.GameRepresentation
{
    public enum PlantType
    {
        Cactus,
        Dandelion,
        Rice,
        Mushroom,
        None
    }

    public enum NutritionType
    {
        Eriktonium,
        Farrtrite,
        Christrogen
    }

    public enum PropagateType
    {
        Cross,          // up down left right
        Square,         // 3x3 square centered around plant
        VerticalUp,     // up 1 layer
        VerticalDown,   // down 1 layer
        VerticalUpCross,
        VerticalDownCross
    }
}