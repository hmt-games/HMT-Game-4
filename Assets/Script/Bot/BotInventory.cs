using System.Collections.Generic;
using UnityEngine;
using GameConstant;

public struct BotInventory {



    public readonly InventoryMode Mode {
        get {
            if (HasReservoir && HasPlantInventory) return InventoryMode.Hybrid;
            if (HasReservoir) return InventoryMode.Reservoir;
            if (HasPlantInventory) return InventoryMode.PlantInventory;
            return InventoryMode.None;
        }
    }
    public readonly bool HasReservoir {
        get { return ReservoirCapacity > 0.0f; }
    }
    public readonly bool HasPlantInventory {
        get { return PlantInventoryCapacity > 0; }
    }
    public readonly float ReservoirCapacity;
    public readonly int PlantInventoryCapacity;
    public NutrientSolution ReservoirInventory;
    public List<PlantStateData> PlantInventory;

    public static BotInventory None {
        get {
            return new BotInventory(0.0f, 0);
        }
    }

    public static BotInventory MakePlantInvetory(int capacity) {
        return new BotInventory(0.0f, capacity);
    }

    public static BotInventory MakePlantInventory(List<PlantStateData> plantInventory) {
        return new BotInventory(0.0f, plantInventory.Count, NutrientSolution.Empty, plantInventory);
    }

    public static BotInventory MakeReservoir(float reservoirCapacity) {
        return new BotInventory(reservoirCapacity, 0);
    }

    public static BotInventory MakeReservoir(float reservoirCapacity, NutrientSolution reservoirInventory) {
        return new BotInventory(reservoirCapacity, 0, reservoirInventory, new List<PlantStateData>());
    }

    public static BotInventory MakeHybridInventory(float reservoirCapacity, int plantInventoryCapacity) {
        return new BotInventory(reservoirCapacity, plantInventoryCapacity);
    }

    public static BotInventory MakeHybridInventory(NutrientSolution reservoirInventory, List<PlantStateData> plantInventory) {
        return new BotInventory(reservoirInventory.water, plantInventory.Count, reservoirInventory, plantInventory);
    }

    private BotInventory(float reservoirCapacity, int plantInventoryCapacity) {
        ReservoirCapacity = reservoirCapacity;
        PlantInventoryCapacity = plantInventoryCapacity;
        ReservoirInventory = NutrientSolution.Empty;
        PlantInventory = new List<PlantStateData>(PlantInventoryCapacity);
    }

    private BotInventory(float reservoirCapacity, int plantInventoryCapacity, NutrientSolution reservoirInventory,
        List<PlantStateData> plantInventory) {
        ReservoirCapacity = reservoirCapacity;
        PlantInventoryCapacity = plantInventoryCapacity;
        ReservoirInventory = reservoirInventory;
        PlantInventory = plantInventory;
    }

    public BotInventory Resize(float reservoirCapacity, int plantInventoryCapacity) {
        if (ReservoirCapacity == reservoirCapacity && PlantInventoryCapacity == plantInventoryCapacity) {
            return this;
        }
        NutrientSolution newReservoir = ReservoirInventory;
        if (reservoirCapacity < ReservoirInventory.water) {
            newReservoir = ReservoirInventory.DrawOff(ReservoirInventory.water - reservoirCapacity);
        }
        List<PlantStateData> newPlantInventory = new List<PlantStateData>(plantInventoryCapacity);
        for (int i = 0; i < Mathf.Min(PlantInventory.Count, plantInventoryCapacity); i++) {
            newPlantInventory.Add(PlantInventory[i]);
        }
        return new BotInventory(reservoirCapacity, plantInventoryCapacity, newReservoir, newPlantInventory);
    }

    public BotInventory Clear() {
        return new BotInventory(ReservoirCapacity, PlantInventoryCapacity);
    }

    public bool IsEmpty => ReservoirInventory.water <= 0.0f && PlantInventory.Count <= 0;

    public bool IsFull => ReservoirInventory.water >= ReservoirCapacity && PlantInventory.Count >= PlantInventoryCapacity;

    public BotInventory AddNutrientSolution(NutrientSolution addition) {
        if (HasReservoir) {
            if (ReservoirInventory.water == ReservoirCapacity) {
                return this;
            }
            else {
                return new BotInventory(ReservoirCapacity, 
                                        0, 
                                        ReservoirInventory + addition.DrawOff(ReservoirCapacity - ReservoirInventory.water), 
                                        new List<PlantStateData>());
            }
        }
        else {
            //Maybe we want to throw an error here instead?
            return this;
        }
    }

    public BotInventory DrawOff(float reservoirSize, int plantSize) {
        NutrientSolution rNutrients = ReservoirInventory.DrawOff(reservoirSize);

        List<PlantStateData> rPlants = new List<PlantStateData>();
        int plantDraw = Mathf.Min(PlantInventory.Count, plantSize);
        for (int i = 0; i < plantDraw; i++) {
            rPlants.Add(PlantInventory[i]);
        }

        for (int i = plantDraw - 1; i >= 0; i--) {
            PlantInventory.RemoveAt(i);
        }

        return new BotInventory(reservoirSize, plantSize, rNutrients, rPlants);
    }

    public void TopOff(ref BotInventory otherInventory) {
        float reservoirTopOff = ReservoirCapacity - ReservoirInventory.water;
        int plantsTopOff = Mathf.Min(PlantInventoryCapacity - PlantInventory.Count, otherInventory.PlantInventory.Count);
        
        ReservoirInventory += otherInventory.ReservoirInventory.DrawOff(reservoirTopOff);

        for (int i = plantsTopOff - 1; i >= 0; i--) {
            PlantInventory.Add(otherInventory.PlantInventory[i]);
            otherInventory.PlantInventory.RemoveAt(i);
        }
    }
}