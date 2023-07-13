using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    private RoomNetwork rn;
    private RoomUIController UIcontroller;
    public int playerSelectedChar = -1;
    public bool isReady = false;

    public static RoomManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance =  this;

            this.UIcontroller = GetComponent<RoomUIController>();
            this.rn = GetComponent<RoomNetwork>();
        }
        else
        {
            Destroy(this);
        }

        // LobbyManager can be destroyed when loading other scene
        //DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        RoomUISetUp();
    }


    // When we first enter room
    public void RoomUISetUp()
    {
        string roomName = this.rn.getRoomName();
        this.UIcontroller.setRoomName(roomName);
        int maxPlayerNum = this.rn.getMaxPlayerNum();
        this.UIcontroller.generateCharOptions(maxPlayerNum);

        // If we are host, there might be something else to display
        if (this.rn.confirmMasterClient())
        {
            Debug.Log("enable Host UI.");
            this.UIcontroller.enableHostUI();
        }
    }

    public void OnBeingHost()
    {
        this.UIcontroller.enableHostUI();
    }

    // When network breaks, return to Lobby level
    public void OnNetworkError()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.LOBBY_LEVEL);
    }

    public void OnPlayerChanged(Dictionary<int, Hashtable> playerList)
    {
        this.UIcontroller.UpdatePlayerList(playerList);
    }

    public void OnCharChanged(HashSet<int> selectedChar, RoomNetwork.AssignmentMode mode)
    {
        this.UIcontroller.UpdateCharCode(selectedChar);

        if (selectedChar.Contains(playerSelectedChar))
        {
            this.UIcontroller.disableReadyButton();
        }
        else if((!isReady && mode != RoomNetwork.AssignmentMode.Choice) || (!isReady && mode == RoomNetwork.AssignmentMode.Choice && playerSelectedChar!= -1))
        {
            this.UIcontroller.enableReadyButton();
        }
    }

    public void LeaveRoomClicked()
    {
        this.rn.TryLeaveRoom();
    }

    public void OnLeaveRoomSucceed()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.LOBBY_LEVEL);
    }

    public void ReadyClicked()
    {
        this.rn.TryGetReady(playerSelectedChar);

        // Disable ready button
        this.UIcontroller.disableReadyButton();
        this.isReady = true;
    }

    public void OnPlayersAllReady()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.FIRST_MULTIPLAYER_LEVEL);
    }

    public void ChangeAssignMode(RoomNetwork.AssignmentMode mode)
    {
        this.rn.changeAssignMode(mode);
    }

    public void OnAssignModeChanged(RoomNetwork.AssignmentMode mode)
    {
        this.UIcontroller.setCharChoiceActive(mode);
        this.isReady = false;

        if (mode != RoomNetwork.AssignmentMode.Choice)
        {
            this.playerSelectedChar = -1;
            this.UIcontroller.enableReadyButton();
        }
        else if(this.playerSelectedChar == -1)
        {
            this.UIcontroller.disableReadyButton();
        }
    }

    public void OnCharChoiceClicked(int charCode)
    {
        if (!isReady)
        {
            this.playerSelectedChar = charCode;

            if (this.rn.isCharAvailable(charCode))
            {
                this.UIcontroller.enableReadyButton();
            }
            else
            {
                this.UIcontroller.disableReadyButton();
            }
        }
    }

    public void OnResetProperties(RoomNetwork.AssignmentMode mode)
    {
        this.isReady = false;

        if (mode != RoomNetwork.AssignmentMode.Choice)
        {
            this.playerSelectedChar = -1;
            this.UIcontroller.enableReadyButton();
        }
    }

    public void OnFailedSelectChar()
    {
        Debug.Log("Fail to select the character.");
    }
}
