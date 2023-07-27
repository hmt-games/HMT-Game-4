//Will store a list of all primitive actions
using System;
using UnityEngine;

public interface IPrimitiveRoombaBotActions
{
    public enum RoombaBotMode
    {
        Water,
        Fertilize,
        Harvest,  
    }
    


    public enum ActionStatus
    {
        NotStarted,
        Performing,
        Finished
    }

    void Water(GridNode gridNode, Action<ActionStatus> actionStatus);

    void Fertilize(GridNode gridNode, Action<ActionStatus> actionStatus);

    void Harvest(GridNode gridNode, Action<ActionStatus> actionStatus);

    void ChangeMode(RoombaBotMode roombaBotMode);

    void GotoLoc(Vector2 gridNodeCoordinates);

}
