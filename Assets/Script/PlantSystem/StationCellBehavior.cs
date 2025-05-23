using GameConstant;
using HMT.Puppetry;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public class StationCellBehavior : GridCellBehavior
{
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
        else if (tileType == TileType.ScoreStation)
        {
            GameActions.Instance.Score(bot);
        }

        GameConfigSO gameConfig = GameManager.Instance.gameConfig;
        
        return tileType switch
        {
            TileType.HarvestStation => gameConfig.harvest,
               TileType.SprayAStation 
            or TileType.SprayBStation
            or TileType.SprayCStation
            or TileType.SprayDStation => gameConfig.spray,
            TileType.PluckStation => gameConfig.pick,
            TileType.PlantStation => gameConfig.plant,
            TileType.TillStation => gameConfig.till,
            TileType.SampleStation => gameConfig.sample,
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
