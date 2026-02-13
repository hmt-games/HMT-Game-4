using System;
using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HMT.Puppetry;


public class PlayerNetworkRemoteSync : MonoBehaviour
{
    public RemotePlayerNetworkData NetworkData;

    private FarmBot _focusedBot = null;

    private void Start()
    {
        NetworkManager.Instance.nakamaConnection.Socket.ReceivedMatchState += EnqueueOnReceivedMatchState;
    }

    private void EnqueueOnReceivedMatchState(IMatchState matchState)
    {
        var mainThread = UnityMainThreadDispatcher.Instance();
        mainThread.Enqueue(() => OnReceivedMatchState(matchState));
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        // If the incoming data is not related to this remote player, ignore it
        if (matchState.UserPresence.SessionId != NetworkData.User.SessionId)
        {
            return;
        }

        switch (matchState.OpCode)
        {
            case OpCodes.FocusBot:
                ReceiveRemoteFocusBot(matchState.State);
                break;
            case OpCodes.UnFocusBot:
                ReceiveRemoteUnFocusBot();
                break;
            case OpCodes.Move:
                ReceiveRemoteMove(matchState.State);
                break;
            case OpCodes.ParamsAction:
                ReceiveRemoteParamsAction(matchState.State);
                break;
            default:
                Debug.LogError($"received match state with unimplemented OpCode {matchState.OpCode}");
                break;
        }
    }

    private void ReceiveRemoteFocusBot(byte[] state)
    {
        string puppetID = System.Text.Encoding.UTF8.GetString(state);
        _focusedBot = GameManager.Instance.puppetID2FarmBot[puppetID];
    }

    private void ReceiveRemoteUnFocusBot()
    {
        if (_focusedBot == null)
        {
            Debug.LogError("Received remote unfocus bot match state when focusBot=null");
            return;
        }

        _focusedBot = null;
    }

    private void ReceiveRemoteMove(byte[] state)
    {
        if (_focusedBot == null)
        {
            Debug.LogError("Trying to move when remote player does not have a focused bot");
        }
        
        string direction = System.Text.Encoding.UTF8.GetString(state);
        HMTPuppetManager.Instance.EnqueueCommand(
            new PuppetCommand(_focusedBot.PuppetID, 
                "move",
                new Newtonsoft.Json.Linq.JObject {
                    { "direction", direction },    
                },
                128));
    }

    private void ReceiveRemoteParamsAction(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);

        string action = stateDictionary["action"];
        int target = Int32.Parse(stateDictionary["target"]);

        HMTPuppetManager.Instance.EnqueueCommand(
            new PuppetCommand(_focusedBot.PuppetID, action,
                new Newtonsoft.Json.Linq.JObject
                {
                    {"target", target}
                },
                128));
    }
    
    private IDictionary<string, string> GetStateAsDictionary(byte[] state)
    {
        return Encoding.UTF8.GetString(state).FromJson<Dictionary<string, string>>();
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.nakamaConnection.Socket.ReceivedMatchState -= EnqueueOnReceivedMatchState;
        }
    }
}
