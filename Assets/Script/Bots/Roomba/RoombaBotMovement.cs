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

    protected util.GridRepresentation.GridLayer currentLayerForRoombaBot;
    
    
    public RoombaBotPathFinder pathFinder = new RoombaBotPathFinder();


    //On reached destination
    protected Action OnReachedDestination;

    //On new destination assigned  
    protected Action OnNewDestinationAssigned;

    public abstract void Moveto(Vector2 gridNodeCoordinates);




    //Get GridNode under the roomba bot. Just in case.
    RaycastHit hitInfo;
    Ray ray;
    protected GridNode GridNodeUnderBot(int layerMask = 3)
    {
        ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out hitInfo, 15f, 1 << layerMask))
        {
            return hitInfo.collider.gameObject.GetComponent<GridNode>();
        }

        return null;
    }


}
