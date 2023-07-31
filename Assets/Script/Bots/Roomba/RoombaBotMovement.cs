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
    protected float movementSpeed;

    protected util.GridRepresentation.GridLayer currentLayerForRoombaBot;
    

    //On Grid Navigation
    public RoombaBotPathFinder pathFinder = new RoombaBotPathFinder();
    protected List<GridNode> currentRoombaBotPath;


    //On start moving
    public Action OnStartMovement;

    //On reached destination
    public Action OnReachedDestination;



    public virtual void Moveto(Vector2 gridNodeCoordinates)
    {

    }

    public virtual void Moveto(Vector2 gridNodeCoodinates, Action<bool> hasReachedDestination)
    {

    }




    //Get GridNode under the roomba bot. Just in case.
    RaycastHit hitInfo;
    Ray ray;
    protected GridNode GridNodeUnderBot(int layerMask = 3)
    {
        ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out hitInfo, 5f, 1 << layerMask))
        {
            return hitInfo.collider.gameObject.GetComponent<GridNode>();
        }

        return null;
    }




    //Visualize path
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; ;        


        if (currentRoombaBotPath != null)
        {
            Gizmos.color = Color.blue;
            foreach (var item in currentRoombaBotPath)
            {
                Gizmos.DrawSphere(item.gameObject.transform.position + new Vector3(0, 0.5f, 0f), 0.4f);
            }
        }
    }

}
