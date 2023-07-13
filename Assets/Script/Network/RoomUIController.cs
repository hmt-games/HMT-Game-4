using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI roomName;
    [SerializeField]
    private GameObject CharacterChoiceUI;
    [SerializeField]
    private GameObject MoveToLevelUI;
    [SerializeField]
    private GameObject playerView;
    [SerializeField]
    private GameObject charView;
    [SerializeField]
    private Button getReadyButton;
    [SerializeField]
    private Toggle randomAssign;
    [SerializeField]
    private GameObject playerItem;
    [SerializeField]
    private GameObject charBnt;

    private List<GameObject> CharBntList = new List<GameObject>();
    private Dictionary<int, GameObject> playerOnScreen = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        showDefaultUI();
    }

    // showing UI that every player should see when they enter the room
    public void showDefaultUI()
    {
        disableReadyButton();
    }

    public void setRoomName(string name)
    {
        this.roomName.text = name;
    }

    public void generateCharOptions(int maxPlayerNum)
    {
        for (int i = 0; i < maxPlayerNum; i++)
        {
            GameObject charbnt = Instantiate(charBnt, charView.transform);
            CharBntList.Add(charbnt);
            charbnt.GetComponent<CharListItem>().setProperties(GameConstants.Character.CHAR_LIST[i], i, true);
        }
    }

    public void disableReadyButton()
    {
        this.getReadyButton.interactable = false;
    }

    public void enableReadyButton()
    {
        this.getReadyButton.interactable = true;
    }

    public void enableHostUI()
    {
        randomAssign.gameObject.SetActive(true);
    }

    public void disableHostUI()
    {
        randomAssign.gameObject.SetActive(false);
    }

    public void UpdatePlayerList(Dictionary<int, Hashtable> playerList)
    {
        Debug.Log("UI player updating");
        // if anyone leave
        foreach (int ActorNum in playerOnScreen.Keys)
        {
            if (!playerList.ContainsKey(ActorNum))
            {
                GameObject itemToDelete = this.playerOnScreen[ActorNum];
                this.playerOnScreen.Remove(ActorNum);
                Destroy(itemToDelete);
            }
        }

        int i = 0;
        foreach (int ActorNum in playerList.Keys)
        {
            i += 1;

            string playerName = "P" + i.ToString();
            bool isHost = (bool)playerList[ActorNum]["isHost"];
            if (playerOnScreen.ContainsKey(ActorNum))
            {
                playerOnScreen[ActorNum].GetComponent<PlayerListItem>().updateProperties(playerName,
                    isHost);
            }
            else
            {
                bool isLocal = (bool)playerList[ActorNum]["isLocal"];

                playerOnScreen[ActorNum] = Instantiate(playerItem, playerView.transform);
                playerOnScreen[ActorNum].GetComponent<PlayerListItem>().setProperties(ActorNum, playerName,
                    isHost, isLocal);
            }
        }
    }

    public void UpdateCharCode(HashSet<int> selectedChar)
    {
        foreach (GameObject bnt in CharBntList)
        {
            if (selectedChar.Contains(bnt.GetComponent<CharListItem>().charCode))
            {
                bnt.GetComponent<Button>().enabled = false;
            }
            else
            {
                bnt.GetComponent<Button>().enabled = true;
            }
        }
    }

    public void setCharChoiceActive(RoomNetwork.AssignmentMode mode)
    {
        if (mode == RoomNetwork.AssignmentMode.Choice)
        {
            charView.SetActive(true);
        }
        else
        {
            charView.SetActive(false);
        }
    }

    public void SwitchAssignMode()
    {
        if (randomAssign.isOn)
        {
            RoomManager.Instance.ChangeAssignMode(RoomNetwork.AssignmentMode.Random);
        }
        else
        {
            RoomManager.Instance.ChangeAssignMode(RoomNetwork.AssignmentMode.Choice);
        }
    }
}
