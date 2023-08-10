using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should hold characteristics of a bot.
/// Specific types of bot behaviors should inherit from this class.
/// </summary>
public abstract class RoombaBot : MonoBehaviour, IPrimitiveRoombaBotActions
{
    public RoombaBotMovement botMovement 
    {
        get
        {
            return GetComponent<RoombaBotMovement>();
        }

    }


    protected IPrimitiveRoombaBotActions.RoombaBotMode currentMode;
    protected IPrimitiveRoombaBotActions.PrimitiveActions currentActionToPerform = IPrimitiveRoombaBotActions.PrimitiveActions.None;
    protected GridNode currentTargetNodeToPerformActionOn;

    
    public RoombaBotCommandList commandList { get; private set; } = new RoombaBotCommandList();

    
    public bool isRoombaBotSelectedByAPlayer = false;


    public Action OnActionStarted, OnActionCancelled, OnActionFinished;

    

    public void SetAction(IPrimitiveRoombaBotActions.PrimitiveActions action)
    {
        currentActionToPerform = action;
    }

    public void SetTarget(GridNode gridNode)
    {
        currentTargetNodeToPerformActionOn = gridNode;
    }



    public virtual void PerformAction()
    {
        if (currentTargetNodeToPerformActionOn == null)
        {
            Debug.LogError("Please provide a valid target first");
            return;
        }

        OnActionStarted?.Invoke();
    }



    public virtual void StopAction()
    {        
        OnActionCancelled?.Invoke();

        currentActionToPerform = IPrimitiveRoombaBotActions.PrimitiveActions.None;
        currentTargetNodeToPerformActionOn = null;        
    }





    //========== PRIMITIVE ACTIONS ==========

    public virtual IEnumerator Water()
    {
        Debug.Log($"Watering {currentTargetNodeToPerformActionOn.gameObject.name}");
        currentTargetNodeToPerformActionOn.waterLevel += 1;
        yield break;
    }



    public virtual IEnumerator Fertilize()
    {
        Debug.Log($"Fertilizing {currentTargetNodeToPerformActionOn.gameObject.name}");
        currentTargetNodeToPerformActionOn.nutrition[util.GameRepresentation.NutritionType.Eriktonium] += 1f;
        currentTargetNodeToPerformActionOn.nutrition[util.GameRepresentation.NutritionType.Christrogen] += 1f;
        currentTargetNodeToPerformActionOn.nutrition[util.GameRepresentation.NutritionType.Farrtrite] += 1f;
        yield break;
    }



    public virtual IEnumerator Harvest()
    {
        Debug.Log($"Harvesting {currentTargetNodeToPerformActionOn.gameObject.name}");
        yield break;
    }



    public virtual IEnumerator ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode roombaBotMode)
    {
        if (currentMode == roombaBotMode)
        {
            Debug.LogError($"Bot is already in {roombaBotMode} Mode. Not changing mode");            
        }
        
        Debug.Log($"Mode Changed to  {roombaBotMode}");
        currentMode = roombaBotMode;
        
        yield break;
    }


    public virtual void Idle()
    {
        Debug.Log("Entered Idle state");
    }


    public virtual IEnumerator GotoLoc(Vector2 gridNodeCoordinates)
    {
        Debug.Log($"Moving to {gridNodeCoordinates}");
        yield return botMovement.Moveto(gridNodeCoordinates);
        yield break;
    }
}





// ====== COMMAND LIST =====
public class RoombaBotCommandList
{
    public List<IPrimitiveRoombaBotActions> roombaBotCommandList = new List<IPrimitiveRoombaBotActions>();



    public List<IPrimitiveRoombaBotActions> GetAllCommands()
    {
        return roombaBotCommandList;
    }



    public void AddCommand(IPrimitiveRoombaBotActions primitiveAction)
    {
        roombaBotCommandList.Add(primitiveAction);
    }



    public void RemoveCommand(IPrimitiveRoombaBotActions primitiveAction)
    {
        foreach (var action in roombaBotCommandList)
        {
            if (action == primitiveAction)
            {
                roombaBotCommandList.Remove(action);                
            }
        }

    }



    IPrimitiveRoombaBotActions nextCommand;
    public IPrimitiveRoombaBotActions GetNextAction()
    {        
        if(roombaBotCommandList.Count > 0)
        {
            nextCommand = roombaBotCommandList[0];
            roombaBotCommandList.RemoveAt(0);
            return nextCommand;
        }

        return null;
    }
}
