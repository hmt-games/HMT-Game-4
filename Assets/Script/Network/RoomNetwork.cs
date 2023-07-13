using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomNetwork : MonoBehaviourPunCallbacks
{
    public bool activeLeave = false;
    public int readyPlayerNum = 0;

    private static string NOTIFY_PLAYER_READY_FN_NAME = "NotifyPlayerReady";
    private static string NOTIFY_ALL_RESET_FN_NAME = "OnReset";
    private static string CHANGE_ASSIGN_MODE_FN_NAME = "OnAssignModeChanged";
    private static string NOTIFY_PROPERTY_UPDATE_FAILED_FN_NAME = "OnFailedUpdateProperties";
    // Which char assign mode are we in
    public enum AssignmentMode
    {
        Static,
        Random,
        Choice
    }

    // Room properties to be synchronize
    private AssignmentMode assignMode = AssignmentMode.Choice;
    //// Local copy of the player property hashtables. ActorNumber is the key. This is for local use.
    private Dictionary<int, Hashtable> playerList = new Dictionary<int, Hashtable>();
    //// A set for selected charcode
    private HashSet<int> selectedChar = new HashSet<int>();
    //// For now, properties to set online is only player's character
    private List<string> PlayerProperties = new List<string>()
    {
        "CharCode"
    };
    private int defaultPropertyValue = -1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("My UserId: " + PhotonNetwork.LocalPlayer.UserId);
        Debug.Log("Am I the master: " + PhotonNetwork.IsMasterClient);

        // Initialize local player properties
        setPlayerProperties(PhotonNetwork.LocalPlayer.ActorNumber, generatePropertyTable(defaultPropertyValue));
        updateLocalLists();
    }

    public void setPlayerProperties(int ActorNum, Hashtable properties)
    {
        var localProperties = PhotonNetwork.CurrentRoom.GetPlayer(ActorNum).CustomProperties;

        foreach (string key in properties.Keys)
        {
            if (localProperties.ContainsKey(key))
            {
                localProperties[key] = properties[key];
            }
            else
            {
                localProperties.Add(key, properties[key]);
            }
        }

        Debug.Log("Set player " + ActorNum + " CharCode to " + properties["CharCode"]);
        PhotonNetwork.CurrentRoom.GetPlayer(ActorNum).SetCustomProperties(localProperties);
    }

    // When player property (CharCode) set successfully, update the cached playerList
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // only host do the updating and then broadcast to other players
        if (PhotonNetwork.IsMasterClient)
        {
            int actorNum = targetPlayer.ActorNumber;
            int charCode = (int)changedProps["CharCode"];

            if (assignMode == AssignmentMode.Choice)
            {
                if (selectedChar.Contains(charCode))
                {
                    readyPlayerNum -= 1;
                    Debug.Log("Warning: A player selected an unavailable character. ");

                    photonView.RPC(NOTIFY_PROPERTY_UPDATE_FAILED_FN_NAME, targetPlayer);
                }
            }

            this.checkReadyConditions();
        }
        updateLocalLists();
    }

    public string getRoomName()
    {
        Debug.Log("Room Name: " + PhotonNetwork.CurrentRoom.Name);
        return PhotonNetwork.CurrentRoom.Name;
    }

    public int getMaxPlayerNum()
    {
        Debug.Log("MaxPlayerNum: " + (int)PhotonNetwork.CurrentRoom.MaxPlayers);
        return (int)PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public bool confirmMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public bool isCharAvailable(int code)
    {
        Debug.Log("This char is available: " + !selectedChar.Contains(code));
        return !selectedChar.Contains(code);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected or failed to build connection.");
        RoomManager.Instance.OnNetworkError();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Someone new here!");

        // if the room is full, set room property so no other player will join
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("Someone left!");
        Debug.Log("Am I the master: " + PhotonNetwork.IsMasterClient);

        // if the room is not full, set room property so other player cans join
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers && !PhotonNetwork.CurrentRoom.IsOpen)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        // if we are the host, delete the player left and the charcode chose by the player
        if (PhotonNetwork.IsMasterClient)
        {
            if ((bool)this.playerList[otherPlayer.ActorNumber]["isHost"])
            {
                readyPlayerNum = 0;

                photonView.RPC(NOTIFY_ALL_RESET_FN_NAME, RpcTarget.All);
                RoomManager.Instance.OnBeingHost();
            }
        }
    }

    public void TryLeaveRoom()
    {
        Debug.Log("Try leave room.");
        this.activeLeave = true;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if (this.activeLeave)
        {
            Debug.Log("Left the room.");
            Debug.Log("Am I the master: " + PhotonNetwork.IsMasterClient);
            this.activeLeave = false;
            RoomManager.Instance.OnLeaveRoomSucceed();
        }

        // Clear local cache
        assignMode = AssignmentMode.Choice;
        playerList.Clear();
        selectedChar.Clear();
    }

    public void TryGetReady(int charCode)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            this.readyPlayerNum += 1;
            this.setPlayerProperties(PhotonNetwork.LocalPlayer.ActorNumber, this.generatePropertyTable(charCode));
        }
        else
        {
            photonView.RPC(NOTIFY_PLAYER_READY_FN_NAME, RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, charCode);
        }
    }

    public void OnOtherPlayerGetReady(int actorNum, int charCode)
    {
        this.readyPlayerNum += 1;

        this.setPlayerProperties(actorNum, this.generatePropertyTable(charCode));
    }

    public void checkReadyConditions()
    {
        if (readyPlayerNum == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            // All players are ready. Prepare to move to level.
            Debug.Log("Move to level.");
            RoomManager.Instance.OnPlayersAllReady();
        }
    }

    public void changeAssignMode(AssignmentMode mode)
    {
        if (assignMode == AssignmentMode.Choice && mode != assignMode)
        {
            photonView.RPC(NOTIFY_ALL_RESET_FN_NAME, RpcTarget.All);
        }

        photonView.RPC(CHANGE_ASSIGN_MODE_FN_NAME, RpcTarget.AllBuffered, mode);
    }

    public void updateLocalLists()
    {
        selectedChar.Clear();
        playerList.Clear();

        foreach (int actorNum in PhotonNetwork.CurrentRoom.Players.Keys)
        {
            //Debug.Log("Updating info of Player " + actorNum);
            playerList[actorNum] = new Hashtable();
            int charCode = (int)PhotonNetwork.CurrentRoom.GetPlayer(actorNum).CustomProperties["CharCode"];
            playerList[actorNum]["CharCode"] = charCode;

            if (charCode != -1)
            {
                selectedChar.Add(charCode);
            }
            if (actorNum != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerList[actorNum]["isLocal"] = false;
            }
            else
            {
                playerList[actorNum]["isLocal"] = true;
            }
            if (actorNum != PhotonNetwork.MasterClient.ActorNumber)
            {
                playerList[actorNum]["isHost"] = false;
            }
            else
            {
                playerList[actorNum]["isHost"] = true;
            }
        }

        RoomManager.Instance.OnPlayerChanged(playerList);
        RoomManager.Instance.OnCharChanged(selectedChar, assignMode);
    }

    // To tell masterclient that local player is ready
    [PunRPC]
    void NotifyPlayerReady(int actorNum, int charCode)
    {
        this.OnOtherPlayerGetReady(actorNum, charCode);
    }

    [PunRPC]
    void OnFailedUpdateProperties()
    {
        setPlayerProperties(PhotonNetwork.LocalPlayer.ActorNumber, generatePropertyTable(defaultPropertyValue));
        RoomManager.Instance.OnResetProperties(this.assignMode);
        RoomManager.Instance.OnFailedSelectChar();
    }

    [PunRPC]
    void OnAssignModeChanged(AssignmentMode mode)
    {
        this.assignMode = mode;
        this.readyPlayerNum = 0;

        RoomManager.Instance.OnAssignModeChanged(mode);
    }

    [PunRPC]
    void OnReset()
    {
        setPlayerProperties(PhotonNetwork.LocalPlayer.ActorNumber, generatePropertyTable(defaultPropertyValue));
        RoomManager.Instance.OnResetProperties(this.assignMode);
    }

    private Hashtable generatePropertyTable(int charCode)
    {
        Hashtable ht = new Hashtable();
        for(int i = 0; i < PlayerProperties.Count; i ++)
        {
            ht[PlayerProperties[i]] = charCode;
        }

        return ht;
    }
}
