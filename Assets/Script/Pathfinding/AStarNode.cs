using UnityEngine;

public class AStarNode : IHeapItem<AStarNode>
{
    public Vector3 NodePosition;
    public bool Walkable;
    public int GridX;
    public int GridY;
    public AStarNode Parent;
    
    public int gCost = 0;
    public int hCost = 0; 
    public int fCost => gCost + hCost;

    private int _heapIndex;

    public AStarNode(Vector3 nodePosition, bool walkable, int gridX, int gridY)
    {
        NodePosition = nodePosition;
        Walkable = walkable;
        GridX = gridX;
        GridY = gridY;
    }

    public int CompareTo(AStarNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare;
    }

    public int HeapIndex
    {
        get => _heapIndex;
        set => _heapIndex = value;
    }
}
