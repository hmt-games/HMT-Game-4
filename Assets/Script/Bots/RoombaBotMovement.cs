using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Each bot might have its own sepcific type of movement.
/// Specific types of mmovement should be inherited from this class.
/// </summary>
[RequireComponent(typeof(RoombaBot))]
public abstract class RoombaBotMovement : MonoBehaviour
{
    [SerializeField]
    protected float movementSpeed = 1f;



    public GridNode currentGridNodeUnderBot;
    
    public RoombaBotPathFinder pathFinder = new RoombaBotPathFinder();



    //On reached destination
    protected Action OnReachedDestination;

    //On new destination assigned  
    protected Action OnNewDestinationAssigned;

    public abstract void Moveto(Vector3 gridNodePositionInWorldSpace);

}
