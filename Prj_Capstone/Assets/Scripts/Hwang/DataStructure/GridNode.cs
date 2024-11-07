using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public Vector3Int hexgridPosition;
    public Vector3Int cellgridPosition;
    public Vector3 worldgridPosition;

    public float gCost;
    public float hCost;
    public float fCost;
    public bool isObstacle;

    public GridNode cameFromNode;
    public CustomTileData customTileData;

    public GridNode(Vector3Int hexgridPosition, Vector3Int cellgridPosition, Vector3 worldgridPosition, bool isObstacle, CustomTileData customTileData)
    {
        this.hexgridPosition = hexgridPosition;
        this.cellgridPosition = cellgridPosition;
        this.worldgridPosition = worldgridPosition;
        this.gCost = 0;
        this.hCost = int.MaxValue;
        this.fCost = gCost + hCost;
        this.isObstacle = isObstacle;
        this.customTileData = customTileData;
    }
}
