using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class RoomListItem : MonoBehaviour
{
    public string roomName;
    public string currentPlayerCount;
    public string maxPlayerNum;

    public void setProperties(string rn, string cpc, string mpn)
    {
        this.roomName = rn;
        this.currentPlayerCount = cpc;
        this.maxPlayerNum = mpn;
        setText();
    }

    public void updateProperties(string cpc, string mpn)
    {
        this.currentPlayerCount = cpc;
        this.maxPlayerNum = mpn;
        setText();
    }

    private void setText()
    {
        this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Room name: "
                        + this.roomName + "  " + this.currentPlayerCount + "/" + this.maxPlayerNum;
    }

    public void onClick()
    {
        LobbyManager.Instance.OnRoomItemClicked(this.roomName);
    }

}