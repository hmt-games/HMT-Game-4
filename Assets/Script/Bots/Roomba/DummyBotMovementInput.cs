using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBotMovementInput : MonoBehaviour
{
    [SerializeField]
    RoombaBot roombaBot;

    
    GridNode currentNode;

    RaycastHit destinationNodehitinfo, startNodehitinfo;


    List<GridNode> botPath;


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


            if(botPath != null)
                botPath.Clear();

            
            //Select Destination after start node has been set.
            if (Physics.Raycast(GetRay(), out destinationNodehitinfo))
            {
                if (destinationNodehitinfo.collider.gameObject.GetComponent<GridNode>())
                {
                    GridNode selectedNode = destinationNodehitinfo.collider.gameObject.GetComponent<GridNode>();

                    Debug.Log(selectedNode.gameObject.name + " selected");

                    botPath = roombaBot.botMovement.pathFinder.FindPath(currentNode, selectedNode);
                }
            }

        }                   
    }


    Ray GetRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; ;
        Gizmos.DrawRay(roombaBot.gameObject.transform.position, Vector3.down * 50f);
        


        if (botPath != null)
        {
            Gizmos.color = Color.blue;
            foreach (var item in botPath)
            {
                Gizmos.DrawSphere(item.gameObject.transform.position + new Vector3(0, 0.5f, 0f), 0.4f);
            }
        }
    }
}
