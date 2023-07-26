using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should hold characteristics of a bot.
/// Specific types of bot behaviors should inherit from this class.
/// </summary>
public abstract class RoombaBot : MonoBehaviour, IPrimitiveRoombaBotActions
{
    public RoombaBotMovement botMovement { get; private set; }


    IPrimitiveRoombaBotActions.RoombaBotMode currentMode;

    
    public RoombaBotCommandList commandList { get; private set; } = new RoombaBotCommandList();



    public virtual void Start()
    {
        botMovement = GetComponent<RoombaBotMovement>();
    }




    //========== PRIMITIVE ACTIONS ==========

    public virtual void Water(GridNode gridNode)
    {
        Debug.Log($"Watering {gridNode.gameObject.name}");
    }



    public virtual void Fertilize(GridNode gridNode)
    {
        Debug.Log($"Fertilizing {gridNode.gameObject.name}");
    }



    public virtual void Harvest(GridNode gridNode)
    {
        Debug.Log($"Harvesting {gridNode.gameObject.name}");
    }



    public virtual void ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode roombaBotMode)
    {
        if (currentMode == roombaBotMode)
        {
            Debug.LogError($"Bot is already in {roombaBotMode} Mode. Please assign a new mode");
            return;
        }
        
        Debug.Log($"Mode Changed to  {roombaBotMode}");
        currentMode = roombaBotMode;
    }



    public virtual void GotoLoc(Vector2 gridNodeCoordinates)
    {
        Debug.Log($"Moving to {gridNodeCoordinates}");

        botMovement.Moveto(gridNodeCoordinates);
    }
}





// ====== COMMAND LIST =====
public class RoombaBotCommandList
{
    List<IPrimitiveRoombaBotActions> roombaBotCommandList = new List<IPrimitiveRoombaBotActions>();



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
