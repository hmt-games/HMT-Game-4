using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRoombaBotMovement : RoombaBotMovement
{


    bool roombaBotMovementEnabled;

    private void OnEnable()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if(roombaBotMovementEnabled)
        {
            if (HasReachedPoint(currentTargetPosition))
            {
                //currentTargetPosition = GetNextPosition();
            }
            else
            {   
                transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, movementSpeed * Time.deltaTime);
            }
        }
    }

    public override void Moveto(Vector2 gridNodeCoordinates)
    {
        //Get path
        currentRoombaBotPath = pathFinder.FindPath(GridNodeUnderBot(), currentLayerForRoombaBot.GetGridNodeByCoordinate(gridNodeCoordinates));        

        //Start navigation
                

        //destination = gridNodePositionInWorldSpace;
    }


    

    Vector3 currentTargetPosition;
    void StartRoombaBotMovementOnPath()
    {
        roombaBotMovementEnabled = true; 
    }

    
    //Vector3 GetNextPosition()
    //{   
        
    //}

    bool HasReachedPoint(Vector3 endPoint)
    {
        return Mathf.Abs(Vector3.Distance(transform.position, endPoint)) < 0.001f;
    }

    void ReachedDestination()
    {
        Debug.Log("Reached Destination");
        roombaBotMovementEnabled = false;
    }

}
