using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviour
{
    public int actorNumber;
    public string playerName;
    public bool isHost;
    public bool isLocal;

    public void setProperties(int actorNum, string pn, bool isHost, bool isLocal)
    {
        this.actorNumber = actorNum;
        this.playerName = pn;
        this.isHost = isHost;
        this.isLocal = isLocal;
        setText();
    }

    public void updateProperties(string pn, bool isHost)
    {
        this.playerName = pn;
        this.isHost = isHost;
        setText();
    }

    private void setText()
    {
        string host = "";
        string local = "";
        if (this.isHost)
        {
            host = "Host";
        }
        if (this.isLocal)
        {
            local = "You";
        }

        this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = this.playerName + "  " + host + "  " + local;
    }
}
