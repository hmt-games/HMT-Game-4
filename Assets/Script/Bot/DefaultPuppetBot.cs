using System.Collections;
using System.Collections.Generic;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class DefaultPuppetBot : PuppetBehavior
{
    private GridCellBehavior _currentGrid;

    public override HashSet<string> SupportedActions =>
        new()
        {
            "Pick", "Harvest", "Spray", "Plant",
            "Sample", "Move", "MoveTo"
        };

    public override void ExecuteAction(PuppetCommand command)
    {
        switch (command.Action)
        {
            case "Move":
                break;
        }
    }

    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }

    public override JObject GetState(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }
}
