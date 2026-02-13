using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class PlayerNetworkLocalSync : MonoBehaviour
{
    public static PlayerNetworkLocalSync Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else
        {
            Debug.LogError("We should never have two local player prefabs");
            Destroy(gameObject);
        }
    }

    public void SendLocalFocusBot(FarmBot bot)
    {
        NetworkManager.Instance.SendMatchState(OpCodes.FocusBot, bot.PuppetID);
    }

    public void SendLocalUnFocusBot()
    {
        NetworkManager.Instance.SendMatchState(OpCodes.UnFocusBot, "");
    }

    public void SendLocalMove(string direction)
    {
        NetworkManager.Instance.SendMatchState(OpCodes.Move, direction);
    }

    public void SendLocalParamsAction(string actionString, int target)
    {
        string matchData = MatchDataJson.ActionAndTarget(actionString, target);
        NetworkManager.Instance.SendMatchState(OpCodes.ParamsAction, matchData);
    }
}
