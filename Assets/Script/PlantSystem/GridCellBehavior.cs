using Fusion;
using GameConstant;
using Newtonsoft.Json.Linq;
using HMT.Puppetry;

public abstract class GridCellBehavior : NetworkBehaviour, IPuppetPerceivable
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
    [Networked]
    public int gridX { get; set; }
    //[FormerlySerializedAs("gridY")]
    [Networked]
    public int gridZ { get; set; }

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
            {"floor", parentFloor.floorNumber}
        };
    }
  
}