using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexgridNode
{
    public Vector3Int hexgridPosition;
    public Vector3Int cellgridPosition;

    public int gCost;
    public int hCost;
    public int fCost;
    public bool isObstacle;

    public HexgridNode cameFromNode;

    public HexgridNode(Vector3Int hexgridPosition, Vector3Int cellgridPosition, bool isObstacle)
    {
        this.hexgridPosition = hexgridPosition;
        this.cellgridPosition = cellgridPosition;
        this.gCost = 0;
        this.hCost = int.MaxValue;
        this.fCost = gCost + hCost;
        this.isObstacle = isObstacle;
    }
}
