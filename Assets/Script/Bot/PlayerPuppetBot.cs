using System.Collections.Generic;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using GameConstant;
using UnityEngine;

public class PlayerPuppetBot : FarmPuppetBot
{
    private Dictionary<KeyCode, string> _keyCode2MoveParams = new Dictionary<KeyCode, string>
    {
        { KeyCode.A, "left" },
        { KeyCode.D, "right" },
        { KeyCode.W, "up" },
        { KeyCode.S, "down" }
    };

    public override HashSet<string> CurrentActionSet =>
        new()
        {
            "pick", "harvest", "spray", "plant",
            "sample", "move", "useStation"
        };

    protected virtual void Update()
    {
        MoveInput();
        UseStationInput();
        PerformBotAction();
    }

    #region Player Input

    // human player movement input
    private void MoveInput()
    {
        foreach (var kvp in _keyCode2MoveParams)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                JObject moveParams = new JObject { { "direction", kvp.Value } };
                PuppetCommand moveCmd = new PuppetCommand(PuppetID, "move", moveParams);
                HMTPuppetManager.Instance.EnqueueCommand(moveCmd);
            }
        }
    }

    private void UseStationInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PuppetCommand cmd = new PuppetCommand(PuppetID, "useStation");
            HMTPuppetManager.Instance.EnqueueCommand(cmd);
        }
    }

    private void PerformBotAction()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            string actionString = _botInfo.CurrentBotMode switch
            {
                BotMode.Sample => "sample",
                BotMode.Spray => "spray",
                BotMode.Harvest => "harvest",
                _ => ""
            };
            
            if (actionString.Length == 0) return;
            
            PuppetCommand cmd = new PuppetCommand(PuppetID, actionString);
            HMTPuppetManager.Instance.EnqueueCommand(cmd);
        }
    }

    #endregion

    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }
}
