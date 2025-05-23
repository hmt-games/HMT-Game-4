using System.Collections.Generic;
using UnityEngine;

public struct BotInventory
{
    public readonly bool HasReservoir;
    public readonly bool HasPlantInventory;
    public readonly float ReservoirCapacity;
    public readonly int PlantInventoryCapacity;
    public NutrientSolution ReservoirInventory;
    public List<PlantBehavior> PlantInventory;

    public BotInventory(bool hasReservoir, bool hasPlantInventory, float reservoirCapacity, int plantInventoryCapacity)
    {
        HasReservoir = hasReservoir;
        HasPlantInventory = hasPlantInventory;
        ReservoirCapacity = reservoirCapacity;
        PlantInventoryCapacity = plantInventoryCapacity;
        ReservoirInventory = NutrientSolution.Empty;
        PlantInventory = new List<PlantBehavior>(PlantInventoryCapacity);
    }

    public BotInventory(float reservoirCapacity, int plantInventoryCapacity, NutrientSolution reservoirInventory,
        List<PlantBehavior> plantInventory)
    {
        HasReservoir = reservoirCapacity != 0.0f;
        HasPlantInventory = plantInventoryCapacity != 0;
        ReservoirCapacity = reservoirCapacity;
        PlantInventoryCapacity = plantInventoryCapacity;
        ReservoirInventory = reservoirInventory;
        PlantInventory = plantInventory;
    }

    public BotInventory DrawOff(float reservoirSize, int plantSize)
    {
        NutrientSolution rNutrients = ReservoirInventory.DrawOff(reservoirSize);
        
        List<PlantBehavior> rPlants = new List<PlantBehavior>();
        int plantDraw = Mathf.Min(PlantInventory.Count, plantSize);
        for (int i = 0; i < plantDraw; i++)
        {
            rPlants.Add(PlantInventory[i]);
        }

        for (int i = plantDraw - 1; i >= 0; i--)
        {
            PlantInventory.RemoveAt(i);
        }

        return new BotInventory(reservoirSize, plantSize, rNutrients, rPlants);
    }
    
    public void TopOff(ref BotInventory otherInventory)
    {
        float reservoirTopOff = ReservoirCapacity - ReservoirInventory.water;
        int plantsTopOff = Mathf.Min(PlantInventoryCapacity - PlantInventory.Count, otherInventory.PlantInventory.Count);
        ReservoirInventory += otherInventory.ReservoirInventory.DrawOff(reservoirTopOff);

        for (int i = plantsTopOff - 1; i >= 0; i--)
        {
            PlantInventory.Add(otherInventory.PlantInventory[i]);
            otherInventory.PlantInventory.RemoveAt(i);
        }
    }
}