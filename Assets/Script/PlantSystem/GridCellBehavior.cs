using System;
using GameConstant;
using Newtonsoft.Json.Linq;
using HMT.Puppetry;
using UnityEngine;

public abstract class GridCellBehavior : MonoBehaviour, IPuppetPerceivable
{
    public TileType tileType = TileType.Soil;
    
    /// <summary>
    /// The floor of a tower that this cell is on. 
    /// </summary>
    public Floor parentFloor;

    /// <summary>
    /// The x and y coordinates of the cell in the grid.
    /// Keep it as x,y instead of row, column because that will make it eaiser to think ahout for the agent interface.
    /// </summary>
    public int gridX { get; set; }
    //[FormerlySerializedAs("gridY")]
    public int gridZ { get; set; }

    // true if there is a bot currently on the tile.
    // this flag is checked so that bot will not walk onto another bot
    public bool botOnGrid = false;

    [SerializeField] private GameObject inventoryDropSprite;
    public bool hasInventoryDrop = false;
    private BotInventory InventoryDrop;
    
    public string ObjectID => $"cell_{parentFloor.floorNumber}_{gridX}_{gridZ}";

    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public abstract void OnTick();

    public abstract NutrientSolution OnWater(NutrientSolution volumes);

    public virtual JObject HMTStateRep(HMTStateLevelOfDetail level) {
        return new JObject {
            {"x", gridX},
            {"y", gridZ},
            {"floor", parentFloor.floorNumber},
            {"bot_on_grid", botOnGrid}
        };
    }

    public void DropInventory(BotInventory botInventory)
    {
        hasInventoryDrop = true;
        inventoryDropSprite.SetActive(true);
        InventoryDrop = botInventory;
    }

    public void TakeInventory(ref BotInventory botInventory)
    {
        botInventory.TopOff(ref InventoryDrop);

        if (InventoryDrop.ReservoirInventory.water == 0.0f && InventoryDrop.PlantInventory.Count == 0)
        {
            hasInventoryDrop = false;
            inventoryDropSprite.SetActive(false);
        }
    }
}