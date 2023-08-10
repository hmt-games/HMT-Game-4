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
    //TO DO: Update movement speed with game tick and not Time.deltaTime
    [SerializeField]
    protected float movementSpeed = 1f;

    [Tooltip("Defines the distance between bot and next point on path at which bot will declare as Reached the point")]
    [SerializeField]
    protected float distanceForPointToBeValid = 0.01f;

    [Tooltip("Offset while moving between points on path")]
    [SerializeField]
    protected Vector3 positionOffset = Vector3.zero;
        

    public RoombaBotPathFinder pathFinder = new RoombaBotPathFinder();



    //On started moving
    public Action OnStartMovement;

    //On reached destination
    public Action OnReachedDestination;


    //Move bot function
    public abstract IEnumerator Moveto(Vector2 gridNodeCoordinates);


    //Reached End function
    public abstract void ReachedDestination();


    //Get next position
    protected abstract Vector3 GetNextPosition();


    //Has reached end of path
    protected abstract bool HasReachedEndOfPath();


    //Has reached current target
    protected abstract bool HasReachedCurrentPoint(Vector3 currentPoint);






    //Get GridNode under the roomba bot. Just in case.
    RaycastHit hitInfo;
    Ray ray;
    protected GridNode GridNodeUnderBot(int layerMask = 3)
    {
        //NOTE: The pivot of the current bot is too low. Raycast does not hit the grid node.
        //Box collider center is a temporary solution. Sorry :(
        Vector3 boxColliderCenter = GetComponent<BoxCollider>().bounds.center;
        ray = new Ray(boxColliderCenter, Vector3.down);
        if(Physics.Raycast(ray, out hitInfo, 15f, 1 << layerMask))
        {
            return hitInfo.collider.gameObject.GetComponent<GridNode>();
        }

        return null;
    }


}
