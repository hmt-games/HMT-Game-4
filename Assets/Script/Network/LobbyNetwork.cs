using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyNetwork : MonoBehaviourPunCallbacks
{
    public float timeBetweenUpdates = 1f;
    private float nextUpdateTime;
    public bool activeLeave = false;

    // This class keeps an up to date list for available rooms in the lobby
    private Dictionary<string, Hashtable> cachedRoomList = new Dictionary<string, Hashtable>();
    private HashSet<string> roomsChanged = new HashSet<string>();

    // Try to connect to server
    public void TryConnectToServer()
    {
        Debug.Log("Try connect to server.");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    // If succeeds
    public override void OnConnectedToMaster()
    {
        Debug.Log("Successfully connected to server.");
        LobbyManager.Instance.OnNetworkConnectSucceed();
    }

    //If fails
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (this.activeLeave)
        {
            Debug.Log("Successfully disconnected.");
            LobbyManager.Instance.OnDisconnectSucceed();
            this.activeLeave = false;
        }
        else
        {
            Debug.Log("Disconnected or failed to build connection.");
            LobbyManager.Instance.OnNetworkError();
        }
    }

    // Try to join lobby
    public void TryJoinLobby()
    {
        Debug.Log("Try join lobby.");
        PhotonNetwork.JoinLobby();
    }

    // If succeeds (JoinLobby doesn't fail since it will create one if there's no lobby)
    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined a lobby.");
        LobbyManager.Instance.OnJoinLobbySucceed();
    }

    // In case player leave a lobby, clear local room record
    public override void OnLeftLobby()
    {
        if (this.activeLeave)
        {
            Debug.Log("Successfully left lobby.");
            this.activeLeave = false;
            LobbyManager.Instance.OnLeaveLobbySucceed();
        }

        cachedRoomList.Clear();
    }

    // When room list updated
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // If player wants to join a room but there is no room available. Remember to set room.open = false when game starts or room is full in RoomManager.
        if (cachedRoomList.Count == 0 && roomList.Count == 0 && LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.JoinRoom)
        {
            LobbyManager.Instance.OnNoAvailableRoom();
        }

        // Other cases. Update room info every timeBetweenUpdates seconds.
        if (Time.time >= nextUpdateTime && (LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.JoinRoom || LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.CreateOrJoinRoom))
        {
            storeRoomChanges(roomList);

            // roomsChanges stores room names of rooms appeared in the list
            HashSet<string> roomschanged = roomsChanged;

            // Update all the changes happens in this period of time
            LobbyManager.Instance.OnUpdateRoomList(cachedRoomList, roomschanged);
            roomsChanged.Clear();
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
        else
        {
            storeRoomChanges(roomList);
        }
    }

    private void storeRoomChanges(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            // Check if the room is new
            // If so, update or delete. If not, add new room info.
            if (cachedRoomList.ContainsKey(room.Name))
            {
                roomsChanged.Add(room.Name);
                if (room.IsOpen == false || room.IsVisible == false)
                {
                    cachedRoomList.Remove(room.Name);
                }
                else
                {
                    cachedRoomList[room.Name]["currentPlayerCount"] = room.PlayerCount;
                    cachedRoomList[room.Name]["maxPlayerNum"] = room.MaxPlayers;
                }
            }
            else
            {
                cachedRoomList[room.Name] = new Hashtable();
                cachedRoomList[room.Name]["currentPlayerCount"] = room.PlayerCount;
                cachedRoomList[room.Name]["maxPlayerNum"] = room.MaxPlayers;
            }
        }
    }

    // Try to join a random room
    public void TryJoinRandomRoom()
    {
        Debug.Log("Try to join a random room.");
        PhotonNetwork.JoinRandomRoom();
    }

    // If succeeds
    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined a room. Send player to the room interface.");
        LobbyManager.Instance.OnEnteredARoom();
    }

    //If fails
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Fail to join a random room.");
        LobbyManager.Instance.OnNoAvailableRoom();
    }

    // Try to join a room with name
    public void TryJoinRoom(string roomName)
    {
        Debug.Log("Try to join a room with name.");
        PhotonNetwork.JoinRoom(roomName);
    }

    // If fails
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Fail to join a room with name.");
        LobbyManager.Instance.OnJoinRoomWithNameFailed();
    }

    // Try to create a room
    public void TryCreateRoom(string roomName, int roomMaxPlayerNum)
    {
        Debug.Log("Try create a new room.");
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = roomMaxPlayerNum });
    }

    // If succeeds
    public override void OnCreatedRoom()
    {
        Debug.Log("Successfully created a room. Send player to the room interface.");
        LobbyManager.Instance.OnEnteredARoom();
    }

    //If fails
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Fail to create a room.");
        LobbyManager.Instance.OnCreateRoomFailed();
    }

    // Try to disconnect. Seems it won't fails?
    public void TryDisconnect()
    {
        this.activeLeave = true;
        Debug.Log("Try disconnect.");
        PhotonNetwork.Disconnect();
    }

    // Try to leave lobby. 
    public void TryLeaveLobby()
    {
        this.activeLeave = true;
        Debug.Log("Try leave lobby.");
        PhotonNetwork.LeaveLobby();
    }
}
