using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridInteractionManager : MonoBehaviour
{

    public static GridInteractionManager gridInteractionManagerInstance;

    public GridInteractionManager()
    {
        gridInteractionManagerInstance = this;
    }



    [SerializeField]
    LayerMask gridLayerMask;

    [SerializeField]
    float gridInteractionDistanceFromCamera = 50f;


    public util.GridRepresentation.GridLayer currentGridLayer; //{ get; private set; }


    //Unity Event
    [SerializeField]
    UnityEvent OnGridNodeSelected_UE, OnGridNodeHover_UE;

    //System Action
    public static Action<GridNode> OnGridNodeSelected_A;


    GridNode currentSelectedGridNode, currentHoverGridNode;


    public bool checkForGrid { get; private set; }

    RaycastHit hitInfo;




    private void OnEnable()
    {
        RoombaBotInteractionUIManager.OnActionSelected_NoParam += CheckForGrid;
        CameraManager.OnCameraChangedToLayer += (int layerIndex) => { currentGridLayer = GameConstants.GameMap.allGridLayers[layerIndex]; };
    }

    private void OnDisable()
    {
        RoombaBotInteractionUIManager.OnActionSelected_NoParam -= CheckForGrid;
    }


    // Update is called once per frame
    void Update()
    {
        if (checkForGrid)
        {
            //Clicked on a grid node
            if (Physics.Raycast(GetRay(), out hitInfo, gridInteractionDistanceFromCamera, gridLayerMask))
            {
                if (hitInfo.collider.gameObject.GetComponent<GridNode>() != null)
                {
                    if(hitInfo.collider.gameObject.GetComponent<GridNode>() != currentHoverGridNode)
                    {
                        currentHoverGridNode = hitInfo.collider.gameObject.GetComponent<GridNode>();
                        OnGridNodeHover_UE?.Invoke();
                    }


                    if (Input.GetMouseButtonDown(0))
                    {
                        currentSelectedGridNode = currentHoverGridNode;                        
                        checkForGrid = false;
                        OnGridNodeSelected_A?.Invoke(currentSelectedGridNode);
                    }
                }


            }
        }
    }


    public void CheckForGrid()
    {
        checkForGrid = true;
    }


    Ray GetRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        if (currentHoverGridNode != null)
            Gizmos.DrawSphere(currentHoverGridNode.gameObject.transform.position, 0.8f);
    }
}
