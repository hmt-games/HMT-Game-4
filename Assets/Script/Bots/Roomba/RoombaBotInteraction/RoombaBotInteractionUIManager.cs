using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows human player to engages with the Bot actions UI. Is Enabled only after a bot has been "selected"
/// </summary>
public class RoombaBotInteractionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject interactionUI;


    public static RoombaBotInteractionUIManager interactionUIInstance;

    public RoombaBotInteractionUIManager()
    {
        interactionUIInstance = this;
    }


    public static Action<IPrimitiveRoombaBotActions.PrimitiveActions> OnActionSelected;
    public static Action OnActionSelected_NoParam;
    

    private void OnEnable()
    {
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoombaBotSelected_A += OpenInteractionUI;
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoomBotUnselected_A += CloseInteractionUI;
    }


    private void OnDisable()
    {
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoombaBotSelected_A -= OpenInteractionUI;
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoomBotUnselected_A -= CloseInteractionUI;
    }



    public void OpenInteractionUI()
    {
        interactionUI.SetActive(true);
    }


    public void CloseInteractionUI()
    {
        interactionUI.SetActive(false);
    }
   

    //===== UI Action Buttons OnClick =====
    public void Water()
    {
        OnActionSelected?.Invoke(IPrimitiveRoombaBotActions.PrimitiveActions.Water);
        OnActionSelected_NoParam?.Invoke();
    }


    public void Fertilize()
    {
        OnActionSelected?.Invoke(IPrimitiveRoombaBotActions.PrimitiveActions.Fertilize);
        OnActionSelected_NoParam?.Invoke();
    }


    public void Harvest()
    {
        OnActionSelected?.Invoke(IPrimitiveRoombaBotActions.PrimitiveActions.Harvest);
        OnActionSelected_NoParam?.Invoke();
    }
}
