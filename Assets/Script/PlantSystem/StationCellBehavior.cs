using GameConstant;
using HMT.Puppetry;

public class StationCellBehavior : GridCellBehavior
{
    public override void OnTick()
    {
        // just do nothing for now
    }

    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        return volumes;
    }

    public void OnUseStation(PuppetBehavior bot)
    {
        
    }
}
