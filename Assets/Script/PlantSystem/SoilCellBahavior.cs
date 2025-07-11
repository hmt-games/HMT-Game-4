using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;

public class SoilCellBehavior : GridCellBehavior
{
    public List<PlantBehavior> plants;

    public int PlantCount { get => plants.Count; }

    public Transform plantChildContainer;

    /// <summary>
    /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
    /// 
    /// This will also probalby play in to renderer information.
    /// Could also impact bot mobility on the tile
    /// </summary>
    public SoilConfigSO soilConfig;


    //TODO: use this
    public bool[] plantSlotOccupation = new bool[GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE];

    public NutrientSolution NutrientLevels = NutrientSolution.Empty;

    public override bool AllowsDrops => true;

    private void Awake() {
        //NutrientLevels = NutrientSolution.Empty;
    }

    public float RemainingWaterCapacity {
        get {
            return soilConfig.waterCapacity - NutrientLevels.water;
        }
    }



    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public override void OnTick() {
        ///Reconile Excess Water
    
        float rootTotal = 0;
        foreach(PlantBehavior plant in plants) {
            rootTotal += plant.RootMass;
        }
        ///Doll out nutrients / water volunme based on the relative root mass of each plant.
        NutrientSolution aggregate = 
            NutrientLevels.water > soilConfig.waterCapacity 
                ? NutrientLevels.DrawOff(NutrientLevels.water - soilConfig.waterCapacity) 
                : NutrientSolution.Empty;
        if (plants.Count == 0) aggregate += NutrientLevels;
        foreach(PlantBehavior plant in plants) {
            aggregate += plant.OnTick(NutrientLevels * (plant.RootMass / rootTotal));
        }

        NutrientLevels = aggregate;

        // draining excess solution to the next floor (if not bottom floor)
        if (NutrientLevels.water > soilConfig.waterCapacity)
        {
            NutrientSolution excess =
                NutrientLevels.DrawOff(Mathf.Min(soilConfig.drainRate, NutrientLevels.water - soilConfig.waterCapacity));

            //TODO： what happens to the bottom floor?
            if (parentFloor.floorNumber != 0)
            {
                GridCellBehavior bottomGrid =
                    parentFloor.parentTower.floors[parentFloor.floorNumber - 1].Cells[gridX, gridZ];

                bottomGrid.OnWater(excess);
            }
            //TODO: add drain sprite animation to next floor
        }
    }

    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        foreach(PlantBehavior plant in plants.OrderBy(p => -p.SurfaceMass)) {
            volumes = plant.OnSpray(volumes);
        }
        NutrientLevels += volumes;
        return NutrientSolution.Empty;
    }

    public override JObject HMTStateRep(HMTStateLevelOfDetail lod) {
        JObject rep = base.HMTStateRep(lod);
        switch (lod) {
            case HMTStateLevelOfDetail.Full:
                rep["soil_config"] = soilConfig.ToFlatJSON();
                rep["nutrients"] = NutrientLevels.ToFlatJSON();
                goto case HMTStateLevelOfDetail.Visible;
            case HMTStateLevelOfDetail.Visible:
                rep["saturation"] = NutrientLevels.water / soilConfig.waterCapacity;
                rep["plant_count"] = PlantCount;
                rep["plants"] = new JArray(plants.Select(p => p.HMTStateRep(lod)));
                goto case HMTStateLevelOfDetail.Seen;
            case HMTStateLevelOfDetail.Seen:
                rep["cell_type"] = "soil";
                break;
        }
        return rep;
    }

    public void AddPlant(PlantStateData plantState) {
        if (plants.Count >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE) {
            Debug.LogWarning("Attempted to add plant to full soil cell");
            return;
        }
        PlantBehavior newPlant = PrefabPooler.Instance.InstantiatePrefab("plant").GetComponent<PlantBehavior>();
        newPlant.SetPlantState(plantState);
        newPlant.transform.localPosition = Vector3.zero;
        newPlant.parentCell = this;
        plants.Add(newPlant);
    }

    public void AddPlant(PlantBehavior plant) {
        if(plants.Count >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE) {
            Debug.LogWarning("Attempted to add plant to full soil cell");
            return;
        }
        
        plants.Add(plant);
        plant.transform.parent = plantChildContainer.GetChild(transform.childCount-1);
        plant.transform.localPosition = Vector3.zero;
        plant.parentCell = this;
    }

    public PlantBehavior RemovePlant(PlantBehavior plant) {
        if (!plants.Contains(plant)) {
            Debug.LogWarning("Attempted to remove plant that is not in the soil cell");
            return null;
        }

        plants.Remove(plant);
        plant.transform.parent = null;
        plant.parentCell = null;
        return plant;
    }
}