using GameConstant;
using HMT.Puppetry;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public class StationCellBehavior : GridCellBehavior
{
    [SerializeField] private BotModeSO harvestBotModeSO;
    [SerializeField] private BotModeSO sprayBotModeSO;
    [SerializeField] private BotModeSO pickBotModeSO;
    [SerializeField] private BotModeSO plantBotModeSO;
    [SerializeField] private BotModeSO tillBotModeSO;
    [SerializeField] private BotModeSO sampleBotModeSO;
    [SerializeField] private BotModeSO carryBotModeSO;
    
    public BotModeSO Interact(FarmPuppetBot bot)
    {
        if (tileType == TileType.SprayAStation | tileType == TileType.SprayBStation |
            tileType == TileType.SprayCStation | tileType == TileType.SprayDStation)
        {
            StartCoroutine(bot.SprayUp());
        } 
        else if (tileType == TileType.DiscardStation)
        {
            bot.DumpInventory();
        }

        return tileType switch
        {
            TileType.HarvestStation => harvestBotModeSO,
               TileType.SprayAStation 
            or TileType.SprayBStation
            or TileType.SprayCStation
            or TileType.SprayDStation => sprayBotModeSO,
            TileType.PluckStation => pickBotModeSO,
            TileType.PlantStation => plantBotModeSO,
            TileType.TillStation => tillBotModeSO,
            TileType.SampleStation => sampleBotModeSO,
            TileType.CarryStation => carryBotModeSO,
            _ => null
        };
    }
    
    public override void OnTick()
    {
        // just do nothing for now
    }
    
    // when drained on station tile, we just discard the drained amount?
    // or just pass down to next floor
    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        //TODO we may want to have this return empty (ie no water drains out of a station to lower floors).
        return NutrientSolution.Empty;
    }

    public override JObject HMTStateRep(HMTStateLevelOfDetail lod) {
        JObject rep = base.HMTStateRep(lod);
        switch (lod) {
            case HMTStateLevelOfDetail.Full:
            case HMTStateLevelOfDetail.Visible:
            case HMTStateLevelOfDetail.Seen:
            case HMTStateLevelOfDetail.Unseen:
                rep["cell_type"] = "station";
                rep["station_type"] = tileType.ToString();
                break;
        }

        return rep;
    }
}
