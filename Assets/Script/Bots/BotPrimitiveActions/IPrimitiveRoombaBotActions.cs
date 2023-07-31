//Will store a list of all primitive actions
using UnityEngine;

public interface IPrimitiveRoombaBotActions
{
    public enum RoombaBotMode
    {
        Water,
        Fertilize,
        Harvest,        
    }
    

    void Water(GridNode gridNode);

    void Fertilize(GridNode gridNode);

    void Harvest(GridNode gridNode);

    void ChangeMode(RoombaBotMode roombaBotMode);

    void GotoLoc(Vector2 gridNodeCoordinates);

}
