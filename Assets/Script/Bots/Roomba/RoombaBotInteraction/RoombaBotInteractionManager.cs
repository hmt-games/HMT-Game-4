using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//Interaction script is only for the human players to interact with bots. 
public class RoombaBotInteractionManager : MonoBehaviour
{

    public static RoombaBotInteractionManager botInteractionManagerInstance;
    
    public RoombaBotInteractionManager()
    {
        botInteractionManagerInstance = this;
    }



    [SerializeField]
    LayerMask roombaBotLayerMask;

    [SerializeField]
    float botInteractionDistanceFromCamera = 50f;

    [Space(10)]

    //Unity Event
    [SerializeField]
    UnityEvent OnRoombaBotSelected_UE, OnRoomBotUnselected_UE;

    //System Action
    public Action OnRoombaBotSelected_A, OnRoomBotUnselected_A;

    public RoombaBot currentSelectedRoombaBot { get; private set; }
    public IPrimitiveRoombaBotActions.PrimitiveActions currentSelectedAction { get; private set; }


    RaycastHit hitInfo;



    private void OnEnable()
    {
        RoombaBotInteractionUIManager.OnActionSelected += (IPrimitiveRoombaBotActions.PrimitiveActions targetAction) => { currentSelectedAction = targetAction; };
        
        GridInteractionManager.OnGridNodeSelected_A += (GridNode selectedGridnode) => { StartRoombaBotAction(currentSelectedAction, selectedGridnode); };

        OnRoombaBotSelected_A += () => { currentSelectedRoombaBot.OnActionFinished += PlayerUnselectRoombaBot; };
        
    }

    private void OnDisable()
    {
        OnRoombaBotSelected_A -= () => { currentSelectedRoombaBot.OnActionFinished -= PlayerUnselectRoombaBot; };
    }




    // Update is called once per frame
    void Update()
    {
        //If mouse clicked
        if(Input.GetMouseButtonDown(0))
        {
            //Clicked on a bot / Bot selected
            if (Physics.Raycast(GetRay(), out hitInfo, botInteractionDistanceFromCamera, roombaBotLayerMask))
            {
                if (hitInfo.collider.gameObject.GetComponent<RoombaBot>() != null)
                {
                    //If bot has been selected by someone else
                    if (hitInfo.collider.gameObject.GetComponent<RoombaBot>().isRoombaBotSelectedByAPlayer)
                    {
                        return;
                    }

                    PlayerSelectRoombaBot(hitInfo.collider.gameObject.GetComponent<RoombaBot>());
                }

            }
            //Clicked somewhere else / Bot unselected
            else
            {
                //if (currentSelectedRoombaBot != null)
                //    UnselectRoombaBot();
            }
        }
        
        
            
    }



    void AISelectRoombaBot(RoombaBot roombaBot)
    {
        currentSelectedRoombaBot = roombaBot;
        
        //This is optional. Remove it if you want to enable human to give command alongside AI.
        currentSelectedRoombaBot.isRoombaBotSelectedByAPlayer = true;
    }    


    void PlayerSelectRoombaBot(RoombaBot roombaBot)
    {
        currentSelectedRoombaBot = roombaBot;
        currentSelectedRoombaBot.isRoombaBotSelectedByAPlayer = true;

        OnRoombaBotSelected_UE?.Invoke();
        OnRoombaBotSelected_A?.Invoke();
    }

    void PlayerUnselectRoombaBot()
    {
        currentSelectedRoombaBot.isRoombaBotSelectedByAPlayer = false;
        currentSelectedAction = IPrimitiveRoombaBotActions.PrimitiveActions.None;
        currentSelectedRoombaBot = null;

        OnRoomBotUnselected_UE?.Invoke();
        OnRoomBotUnselected_A?.Invoke();
    }

    


    //Set action to perform on the target grid node
    public void StartRoombaBotAction(IPrimitiveRoombaBotActions.PrimitiveActions actionToPerform, GridNode target)
    {
        currentSelectedRoombaBot.SetAction(actionToPerform);
        currentSelectedRoombaBot.SetTarget(target);

        currentSelectedRoombaBot.PerformAction();        
    }



    Ray GetRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}
