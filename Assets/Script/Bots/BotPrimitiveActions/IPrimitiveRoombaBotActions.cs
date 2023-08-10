//Will store a list of all primitive actions.
//Add, Edit, Remove all the primitive actions starting here.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TO DO: Change the name of this interface to better reflect what it does.
public interface IPrimitiveRoombaBotActions 
{
    public enum RoombaBotMode
    {
        Idle,
        Water,
        Fertilize,
        Harvest,        
    }
    
    public enum PrimitiveActions
    {
        None,
        Water,
        Fertilize,
        Harvest,
        ChangeMode,
        GoToLoc
    }



    void Idle();

    IEnumerator Water();

    IEnumerator Fertilize();

    IEnumerator Harvest();

    IEnumerator ChangeMode(RoombaBotMode roombaBotMode);

    IEnumerator GotoLoc(Vector2 gridNodeCoordinates);    
}

