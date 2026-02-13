using System;
using Nakama;
using Nakama.TinyJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public NakamaConnection nakamaConnection;
    [SerializeField] private GameObject NetworkLocalPlayerPrefab;
    [SerializeField] private GameObject NetworkRemotePlayerPrefab;
    
    private IDictionary<string, GameObject> players;
    private IUserPresence localUser;
    private GameObject localPlayer;
    private IMatch currentMatch;

    [SerializeField] private Button findMatchButton;

    public static NetworkManager Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        players = new Dictionary<string, GameObject>();
        
        // (per the official tutorial)
        // Get a reference to the UnityMainThreadDispatcher.
        // We use this to queue event handler callbacks on the main thread.
        // If we did not do this, we would not be able to instantiate objects or manipulate things like UI.
        var mainThread = UnityMainThreadDispatcher.Instance();
        
        await nakamaConnection.Connect();
        
        nakamaConnection.Socket.ReceivedMatchmakerMatched += m => mainThread.Enqueue(() => OnReceivedMatchmakerMatched(m));
        nakamaConnection.Socket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnReceivedMatchPresence(m));
        
        findMatchButton.onClick.AddListener(FindMatch);
        
        Debug.Log("Nakama Connection Complete");
    }
    
    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matched)
    {
        // Cache a reference to the local user.
        localUser = matched.Self.Presence;

        // Join the match.
        var match = await nakamaConnection.Socket.JoinMatchAsync(matched);

        // Spawn a player instance for each connected user.
        foreach (var user in match.Presences)
        {
            SpawnPlayer(match.Id, user);
        }

        // Cache a reference to the current match.
        currentMatch = match;
        
        Debug.Log(currentMatch);
    }

    private void SpawnPlayer(string matchId, IUserPresence user)
    {
        if (players.ContainsKey(user.SessionId)) return;
        
        bool isLocal = user.SessionId == localUser.SessionId;
        
        GameObject playerPrefab = isLocal ? NetworkLocalPlayerPrefab : NetworkRemotePlayerPrefab;
        
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        
        if (!isLocal)
        {
            player.GetComponent<PlayerNetworkRemoteSync>().NetworkData = new RemotePlayerNetworkData
            {
                MatchId = matchId,
                User = user
            };
        }
        
        players.Add(user.SessionId, player);
        
        if (isLocal)
        {
            localPlayer = player;
        }
    }
    
    private void OnReceivedMatchPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        foreach (var user in matchPresenceEvent.Joins)
        {
            SpawnPlayer(matchPresenceEvent.MatchId, user);
        }

        foreach (var user in matchPresenceEvent.Leaves)
        {
            if (players.ContainsKey(user.SessionId))
            {
                Destroy(players[user.SessionId]);
                players.Remove(user.SessionId);
            }
        }
    }
    
    public void SendMatchState(long opCode, string state)
    {
        nakamaConnection.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    private async void FindMatch()
    {
        await nakamaConnection.FindMatch();
    }
}
