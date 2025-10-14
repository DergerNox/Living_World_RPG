using UnityEngine;

/// <summary>
/// Represents one grid‐cell for A* pathfinding.
/// </summary>
public class Node : HeapItem<Node>
{
    public Vector3Int Position { get; private set; }
    public bool Walkable { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public Node Parent { get; set; }
    public int FCost => GCost + HCost;

    public Node(bool walkable, Vector3Int position)
    {
        Walkable = walkable;
        Position = position;
        GCost = int.MaxValue;
        HCost = 0;
        Parent = null;
    }

    public override int CompareTo(Node other)
    {
        int cmp = FCost.CompareTo(other.FCost);
        if (cmp == 0)
            cmp = HCost.CompareTo(other.HCost);
        return cmp;
    }
}
