using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRoombaBot : RoombaBot
{
    
    // Start is called before the first frame update

    public override void Start()
    {
        base.Start();
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }




    //Define the primitive actions in detail here
    public override void Fertilize(GridNode gridNode)
    {        
        base.Fertilize(gridNode);
    }

    public override void Water(GridNode gridNode)
    {
        base.Water(gridNode);
    }


    public override void Harvest(GridNode gridNode)
    {
        base.Harvest(gridNode);
    }


    public override void GotoLoc(Vector2 gridNodeCoordinates)
    {
        base.GotoLoc(gridNodeCoordinates);
    }

    public override void ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode roombaBotMode)
    {
        base.ChangeMode(roombaBotMode);
    }


}
