using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRoombaBotMovement : RoombaBotMovement
{

    List<GridNode> currentRoombaBotPath = new List<GridNode>();

    Vector3 currentTargetPosition;




    public override IEnumerator Moveto(Vector2 gridNodeCoordinates)
    {

        //Set index counter to 0
        positionIndex = 0;

        //Invoke start action
        OnStartMovement?.Invoke();

        //Clear existing path
        if (currentRoombaBotPath.Count > 0)
            currentRoombaBotPath.Clear();


        //Get Path 
        currentRoombaBotPath = pathFinder.FindPath(GridNodeUnderBot(), GridInteractionManager.gridInteractionManagerInstance.currentGridLayer.GetGridNodeByCoordinate(gridNodeCoordinates));

        
        //Set first point
        currentTargetPosition = currentRoombaBotPath[0].gameObject.transform.position + positionOffset;


        //If player selects adjacent node
        if (currentRoombaBotPath.Count == 1)
        {
            transform.rotation = GetLookRotation(currentTargetPosition);
            ReachedDestination();
            yield break;
        }


        while (!HasReachedEndOfPath())
        {

            //Check if reached current target node
            if (HasReachedCurrentPoint(currentTargetPosition))
            {
                currentTargetPosition = GetNextPosition() + positionOffset;
            }
            else
            {
                //Update position
                transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, movementSpeed * Time.deltaTime);

                //Update rotation
                transform.rotation = GetLookRotation(currentTargetPosition);
            }


            yield return new WaitForEndOfFrame();
        }

        ReachedDestination();

        yield break;
    }




    int positionIndex = 0;
    protected override Vector3 GetNextPosition()
    {
        return currentRoombaBotPath[positionIndex++].gameObject.transform.position;
    }


    Quaternion GetLookRotation(Vector3 currentTargetPoint)
    {
        Vector3 lookDirection = currentTargetPoint - transform.position;
        lookDirection = Vector3.ProjectOnPlane(lookDirection, Vector3.up);
        return Quaternion.LookRotation(lookDirection);
    }


    protected override bool HasReachedCurrentPoint(Vector3 somePoint)
    {
        return Mathf.Abs(Vector3.Distance(transform.position, somePoint)) < distanceForPointToBeValid;
    }


    protected override bool HasReachedEndOfPath()
    {
        return positionIndex == (currentRoombaBotPath.Count) ? true : false;
    }


    public override void ReachedDestination()
    {
        Debug.Log("Reached Destination");        

        OnReachedDestination?.Invoke();        
    }

    //Draw Path on Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(currentRoombaBotPath.Count > 0)
        {
            foreach (var point in currentRoombaBotPath)
            {
                Gizmos.DrawSphere(point.gameObject.transform.position + new Vector3(0, 0.5f, 0), 0.4f);
            }
        }
    }
}
