using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRoombaBotMovement : RoombaBotMovement
{
    Vector3 destination;


    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed * Time.deltaTime);
    }

    public override void Moveto(KeyValuePair<int, int> gridNodeCoordinates)
    {
        //destination = gridNodePositionInWorldSpace;
    }

}
