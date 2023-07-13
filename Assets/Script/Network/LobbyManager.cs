using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//This is an example LobbyManager class controlling the user input and user flow logic in lobby
public class LobbyManager : MonoBehaviour
{
    public enum OnboardingState { ChooseGameMode, ChooseJoinRoomMode, CreateRoom, JoinRoom, CreateOrJoinRoom, JoinRandomRoom, Loading, Disconnected };

    public OnboardingState onboardingState = OnboardingState.ChooseGameMode;
    private OnboardingState playerChoice = OnboardingState.ChooseGameMode;
    public static LobbyManager Instance;

    private LobbyUIController UIcontroller;
    private LobbyNetwork ln;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        // LobbyManager can be destroyed when loading other scene
        //DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        this.UIcontroller = GetComponent<LobbyUIController>();
        this.ln = GetComponent<LobbyNetwork>();
    }

    // ChooseGameMode: Choose between local play and online play
    // When player chooses local play, or we want to test locally. Go to local play level1.
    public void LocalPlayModeSelected()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.FIRST_SINGLEPLAYER_LEVEL);
    }

    // When player chooses online multi player mode. Change onboarding state. Connect to server. Show loading UI.
    public void OnlinePlayModeSelected()
    {
        this.onboardingState = OnboardingState.Loading;

        this.ln.TryConnectToServer();
        this.UIcontroller.ShowLoadingUI();
    }

    // When successfully connected to server, move to the next step
    public void OnNetworkConnectSucceed()
    {
        this.onboardingState = OnboardingState.ChooseJoinRoomMode;

        this.UIcontroller.ShowJoinRoomModeUI();
    }

    // When disconnected or fail to connect, back to the beginning and show message to player
    public void OnNetworkError()
    {
        this.onboardingState = OnboardingState.Disconnected;

        this.UIcontroller.ShowDisconnectedUI();
        this.UIcontroller.SetMessage("Your network connecting fails. Please check and try again.");
    }


    // ChooseJoinRoomMode: Choose between creating or joining a room and randomly joining a room
    // When player chooses to create or join room. Change onboarding state. Join lobby on server. Show loading UI.
    public void CreateOrJoinSelected()
    {
        this.onboardingState = OnboardingState.Loading;
        this.playerChoice = OnboardingState.CreateOrJoinRoom;

        this.ln.TryJoinLobby();
        this.UIcontroller.ShowLoadingUI();
    }

    // When player chooses to create room. Change onboarding state. Join lobby on server. Show loading UI.
    public void CreateRoomSelected()
    {
        this.onboardingState = OnboardingState.Loading;
        this.playerChoice = OnboardingState.CreateRoom;

        this.ln.TryJoinLobby();
        this.UIcontroller.ShowLoadingUI();
    }

    // When player chooses to join room. Change onboarding state. Join lobby on server. Show loading UI.
    public void JoinRoomSelected()
    {
        this.onboardingState = OnboardingState.Loading;
        this.playerChoice = OnboardingState.JoinRoom;

        this.ln.TryJoinLobby();
        this.UIcontroller.ShowLoadingUI();
    }

    // When player chooses to randomly join a room. Change onboarding state. Join lobby on server. Show loading UI.
    public void RandomJoinSelected()
    {
        this.onboardingState = OnboardingState.Loading;
        this.playerChoice = OnboardingState.JoinRandomRoom;

        this.ln.TryJoinRandomRoom();
        this.UIcontroller.ShowLoadingUI();
    }

    // When cuccessfually joining a lobby. Move to next step.
    public void OnJoinLobbySucceed()
    {
        this.onboardingState = this.playerChoice;

        this.UIcontroller.ShowRoomModeUI();
    }

    // When fails to find an available room. May happen after randomly joining or try to join only in the lobby.
    public void OnNoAvailableRoom()
    {
        // only join randomly put player back to choose the mode again. Others can be done by go back button.
        if (this.playerChoice == OnboardingState.JoinRandomRoom)
        {
            this.onboardingState = OnboardingState.ChooseJoinRoomMode;
            this.UIcontroller.ShowJoinRoomModeUI();
        }

        this.UIcontroller.SetMessage("There is no available room. You can create your own.");
    }

    // When player entered a room
    public void OnEnteredARoom()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.ROOM_LEVEL);
    }

    // When player try to create a room with input. Show loading UI.
    public void CreateRoomClicked()
    {
        string roomName = this.UIcontroller.getNewRoomName();
        int roomMaxPlayerNum = this.UIcontroller.getMaxPlayerNum();

        // Check int input
        if (roomMaxPlayerNum == 0 || roomMaxPlayerNum < GameConstants.GlobalConstants.MIN_NUMBER_OF_PLAYERS)
        {
            this.UIcontroller.SetMessage("Input invaild, or max number of players in room less than " + GameConstants.GlobalConstants.MIN_NUMBER_OF_PLAYERS + ". Please try again.");
        } else if (roomMaxPlayerNum > GameConstants.GlobalConstants.MAX_NUMBER_OF_PLAYERS)
        {
            this.UIcontroller.SetMessage("Max number of players in room more than " + GameConstants.GlobalConstants.MAX_NUMBER_OF_PLAYERS + ". Please try again.");
        }else
        {
            this.onboardingState = OnboardingState.Loading;

            this.ln.TryCreateRoom(roomName, roomMaxPlayerNum);
            this.UIcontroller.ShowLoadingUI();
        }
    }

    // When failed to create a room with input.
    public void OnCreateRoomFailed()
    {
        this.onboardingState = this.playerChoice;
        this.UIcontroller.ShowRoomModeUI();

        this.UIcontroller.SetMessage("Fails to create a room with the name. It may be because a room with the same name exists. Please try again.");
    }

    // When player try to join an existing room. Show loading UI.
    public void JoinRoomClicked()
    {
        string roomName = this.UIcontroller.getRoomNameToJoin();
        if (roomName != "")
        {
            this.onboardingState = OnboardingState.Loading;
            this.ln.TryJoinRoom(roomName);
            this.UIcontroller.ShowLoadingUI();
        }
        else
        {
            this.UIcontroller.SetMessage("You can't join room with an empty name.");
        }
    }

    public void OnJoinRoomWithNameFailed()
    {
        this.onboardingState = this.playerChoice;
        this.UIcontroller.ShowRoomModeUI();

        this.UIcontroller.SetMessage("Fails to join a room with the name. It may be because the room doesn't exist or is already closed. Please try again.");
    }

    // When necessary, update room list UI. If you want a loooong list showing all rooms, change dictionary to list and edit UI code.
    public void OnUpdateRoomList(Dictionary<string, Hashtable> roomList, HashSet<string> roomsChanged)
    {
        this.UIcontroller.UpdateRoomList(roomList, roomsChanged);
    }

    // When one room item clicked
    public void OnRoomItemClicked(string roomName)
    {
        // Tell the UIcontroller to change inputfield input
        this.UIcontroller.UpdateJoinRoomNameInput(roomName);
    }

    public void GoBackClicked()
    {
        switch (onboardingState)
        {
            case OnboardingState.ChooseGameMode:
                this.BackToTitle();
                break;
            case OnboardingState.ChooseJoinRoomMode:
                this.onboardingState = OnboardingState.Loading;
                this.UIcontroller.ShowLoadingUI();
                this.ln.TryDisconnect();
                break;
            case OnboardingState.CreateRoom:
                //this.playerChoice = OnboardingState.CreateRoom;
                this.onboardingState = OnboardingState.Loading;
                this.UIcontroller.ShowLoadingUI();
                this.ln.TryLeaveLobby();
                break;
            case OnboardingState.JoinRoom:
                //this.playerChoice = OnboardingState.JoinRoom;
                this.onboardingState = OnboardingState.Loading;
                this.UIcontroller.ShowLoadingUI();
                this.ln.TryLeaveLobby();
                break;
            case OnboardingState.CreateOrJoinRoom:
                //this.playerChoice = OnboardingState.CreateOrJoinRoom;
                this.onboardingState = OnboardingState.Loading;
                this.UIcontroller.ShowLoadingUI();
                this.ln.TryLeaveLobby();
                break;
            case OnboardingState.Disconnected:
                this.BackToTitle();
                break;
            default:
                Debug.Log("No proper action found for go back.");
                break;
        }
    }

    public void RefreshClicked()
    {
        this.UIcontroller.RefreshRoomList();
    }

    public void OnDisconnectSucceed()
    {
        this.onboardingState = OnboardingState.ChooseGameMode;
        this.UIcontroller.ShowGameModeUI();
    }

    // Seems it won't fail
    //public void OnDisconnectFailed()
    //{
    //    this.onboardingState = OnboardingState.ChooseJoinRoomMode;
    //    this.UIcontroller.ShowJoinRoomModeUI();
    //    this.UIcontroller.SetMessage("Action fails for some reason. ");
    //}

    public void OnLeaveLobbySucceed()
    {
        this.onboardingState = OnboardingState.ChooseJoinRoomMode;
        this.UIcontroller.ShowJoinRoomModeUI();
    }

    // Seems it won't fail
    //public void OnLeaveLobbyFailed()
    //{
    //    this.onboardingState = this.playerChoice;
    //    this.UIcontroller.ShowRoomModeUI();
    //    this.UIcontroller.SetMessage("Action fails for some reason. ");
    //}

    public void BackToTitle()
    {
        SceneManager.LoadScene(GameConstants.GlobalConstants.TITLE_LEVEL);
    }
}
