using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Example class of lobby UI control. Please group the UI widgets for a same function as children of one parent object.
public class LobbyUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameModeUI;
    [SerializeField]
    private GameObject JoinRoomModeUI;
    [SerializeField]
    private GameObject LoadingUI;
    [SerializeField]
    private GameObject CreateRoomUI;
    [SerializeField]
    private GameObject JoinRoomUI;
    [SerializeField]
    private TextMeshProUGUI messageBox;
    [SerializeField]
    private Button goBackButton;
    [SerializeField]
    private TMP_InputField createRoomNameInput;
    [SerializeField]
    private TMP_InputField roomMaxPlayerNumInput;
    [SerializeField]
    private GameObject roomView;
    [SerializeField]
    private TMP_InputField joinRoomNameInput;
    [SerializeField]
    private GameObject roomBnt;

    private Dictionary<string, GameObject> roomsOnScreen = new Dictionary<string, GameObject>();
    private Dictionary<string, Hashtable> cachedRoomList = new Dictionary<string, Hashtable>();

    private void Start()
    {
        ShowGameModeUI();
    }

    public void ShowGameModeUI()
    {
        DisableAllUI();
        GameModeUI.SetActive(true);
        goBackButton.gameObject.SetActive(true);
    }

    public void ShowLoadingUI()
    {
        DisableAllUI();
        LoadingUI.SetActive(true);
    }

    public void ShowJoinRoomModeUI()
    {
        DisableAllUI();
        JoinRoomModeUI.SetActive(true);
        goBackButton.gameObject.SetActive(true);
    }

    public void ShowDisconnectedUI()
    {
        ShowGameModeUI();
    }

    // Show lobby UI based on player's choice
    public void ShowRoomModeUI()
    {
        DisableAllUI();

        if (LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.CreateOrJoinRoom)
        {
            CreateRoomUI.SetActive(true);
            JoinRoomUI.SetActive(true);
            goBackButton.gameObject.SetActive(true);
        }
        else if (LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.CreateRoom)
        {
            CreateRoomUI.SetActive(true);
            goBackButton.gameObject.SetActive(true);
        }
        else if (LobbyManager.Instance.onboardingState == LobbyManager.OnboardingState.JoinRoom)
        {
            JoinRoomUI.SetActive(true);
            goBackButton.gameObject.SetActive(true);
        }
    }

    // Set message for players.
    public void SetMessage(string message)
    {
        messageBox.text = message;
        messageBox.gameObject.SetActive(true);
    }

    private void DisableAllUI()
    {
        GameModeUI.SetActive(false);
        JoinRoomModeUI.SetActive(false);
        LoadingUI.SetActive(false);
        CreateRoomUI.SetActive(false);
        JoinRoomUI.SetActive(false);
        messageBox.gameObject.SetActive(false);
        goBackButton.gameObject.SetActive(false);
    }

    // Functions to send room information to manager
    public string getNewRoomName()
    {
        Debug.Log("Player set room name equals " + createRoomNameInput.text);

        return createRoomNameInput.text;
    }

    public int getMaxPlayerNum()
    {
        string playerInput = roomMaxPlayerNumInput.text;
        int num = 0;

        if (int.TryParse(playerInput, out num))
        {
            Debug.Log("Player set max player number equals " + num);

            return num;
        }

        Debug.Log("Player max player number input invaild");
        return num;
    }

    public string getRoomNameToJoin()
    {
        Debug.Log("Player choose room name equals " + createRoomNameInput.text);

        return joinRoomNameInput.text;
    }

    // to update the room list showing on the screen
    public void UpdateRoomList(Dictionary<string, Hashtable> newRoomList, HashSet<string> roomsChanged)
    {
        this.cachedRoomList = newRoomList;

        foreach(string nameOnScreen in roomsOnScreen.Keys)
        {
            if (roomsChanged.Contains(nameOnScreen))
            {
                if (newRoomList.ContainsKey(nameOnScreen))
                {
                    // Room info changes
                    Hashtable newRoomInfo = newRoomList[nameOnScreen];
                    this.roomsOnScreen[nameOnScreen].GetComponentInChildren<RoomListItem>().updateProperties(newRoomInfo["currentPlayerCount"].ToString(), newRoomInfo["maxPlayerNum"].ToString());
                }
                else
                {
                    // Room doesn't exist anymore, delete button
                    GameObject bntToDelete = this.roomsOnScreen[nameOnScreen];
                    this.roomsOnScreen.Remove(nameOnScreen);
                    Destroy(bntToDelete);
                }
            }
        }

        // If have empty slots and other rooms available to show on screen
        if (this.roomsOnScreen.Count < GameConstants.GlobalConstants.NUMBER_OF_ROOM_SLOTS_IN_LOBBYUI && this.roomsOnScreen.Count < newRoomList.Count)
        {
            foreach (string name in newRoomList.Keys)
            {
                if (!this.roomsOnScreen.ContainsKey(name))
                {
                    Hashtable roomInfo = newRoomList[name];
                    this.roomsOnScreen[name] = Instantiate(roomBnt, roomView.transform);
                    this.roomsOnScreen[name].GetComponentInChildren<RoomListItem>().setProperties(name, roomInfo["currentPlayerCount"].ToString(), roomInfo["maxPlayerNum"].ToString());

                    if (this.roomsOnScreen.Count >= newRoomList.Count || this.roomsOnScreen.Count >= GameConstants.GlobalConstants.NUMBER_OF_ROOM_SLOTS_IN_LOBBYUI)
                    {
                        break;
                    }
                }
            }
        }
    }

    // To refresh the room list for people to join
    public void RefreshRoomList()
    {
        // If there are more rooms available
        if (roomsOnScreen.Count < cachedRoomList.Count)
        {
            // Record old rooms on screen
            HashSet<string> oldRooms = new HashSet<string>(roomsOnScreen.Keys);

            //Delete old buttons
            foreach (GameObject bnt in roomsOnScreen.Values)
            {
                Destroy(bnt);
            }
            roomsOnScreen.Clear();

            // Make a copy of room names for randomly pick rooms which didn't appear on screen before refresh
            List<string> copiedRoomList = new List<string>(cachedRoomList.Keys);

            // Until slots are full or all new rooms are there on screen
            while (roomsOnScreen.Count < GameConstants.GlobalConstants.NUMBER_OF_ROOM_SLOTS_IN_LOBBYUI && roomsOnScreen.Count + oldRooms.Count < cachedRoomList.Count)
            {
                int num = Random.Range(0, copiedRoomList.Count);
                string randomName = copiedRoomList[num];

                if (!oldRooms.Contains(randomName))
                {
                    Hashtable roomInfo = cachedRoomList[randomName];
                    this.roomsOnScreen[randomName] = Instantiate(roomBnt, roomView.transform);
                    this.roomsOnScreen[randomName].GetComponentInChildren<RoomListItem>().setProperties(randomName, roomInfo["currentPlayerCount"].ToString(), roomInfo["maxPlayerNum"].ToString());

                    copiedRoomList.RemoveAt(num);
                }
                else
                {
                    copiedRoomList.RemoveAt(num);
                }
            }
        }
    }

    public void UpdateJoinRoomNameInput(string name)
    {
        this.joinRoomNameInput.text = name;
    }

}
