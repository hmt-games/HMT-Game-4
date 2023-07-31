using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBotMovementInput : MonoBehaviour
{
    [SerializeField]
    RoombaBot roombaBot;

    
    GridNode currentNode;

    RaycastHit destinationNodehitinfo, startNodehitinfo;



    private void OnEnable()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //Set the Current node
            if (Physics.Raycast(roombaBot.gameObject.transform.position, Vector3.down, out startNodehitinfo, 20f))
            {
                currentNode = startNodehitinfo.collider.gameObject.GetComponent<GridNode>();                
            }
            else
                return;

            
            //Select Destination after start node has been set.
            if (Physics.Raycast(GetRay(), out destinationNodehitinfo))
            {
                if (destinationNodehitinfo.collider.gameObject.GetComponent<GridNode>())
                {
                    GridNode selectedNode = destinationNodehitinfo.collider.gameObject.GetComponent<GridNode>();

                    Debug.Log(selectedNode.gameObject.name + " selected");

                    roombaBot.botMovement.Moveto(selectedNode.coordinate);
                }
            }

        }                   
    }


    Ray GetRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
    
}
