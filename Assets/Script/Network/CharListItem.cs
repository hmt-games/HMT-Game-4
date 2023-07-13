using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharListItem : MonoBehaviour
{
    public string charName;
    public int charCode;
    public bool isAvailable;

    public void setProperties(string cn, int cc, bool isavailable)
    {
        this.charName = cn;
        this.charCode = cc;
        this.isAvailable = isavailable;
        setText();
    }

    public void updateProperties(bool isavailable)
    {
        this.isAvailable = isavailable;
        setText();
    }

    private void setText()
    {
        this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = this.charName;
        if (isAvailable)
        {
            this.gameObject.GetComponent<Button>().enabled = true;
        }
        else
        {
            this.gameObject.GetComponent<Button>().enabled = false;
        }
    }

    public void onClick()
    {
        RoomManager.Instance.OnCharChoiceClicked(this.charCode);
    }
}
