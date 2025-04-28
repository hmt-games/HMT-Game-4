using GameConstant;
using HMT.Puppetry;
using UnityEngine;

public class StationCellBehavior : GridCellBehavior
{
    [SerializeField] private BotModeSO harvestBotModeSO;
    [SerializeField] private BotModeSO sprayBotModeSO;
    [SerializeField] private BotModeSO pluckBotModeSO;
    [SerializeField] private BotModeSO tillBotModeSO;
    [SerializeField] private BotModeSO sampleBotModeSO;
    [SerializeField] private BotModeSO carryBotModeSO;
    
    public BotModeSO UseStation(FarmPuppetBot bot)
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
            TileType.PluckStation => pluckBotModeSO,
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
    
    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        return volumes;
    }
}
