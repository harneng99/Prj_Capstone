using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    [field: SerializeField] public Grid gridBase { get; private set; }
    [field: SerializeField] public Tilemap moveableTilemap { get; private set; }
    
    [HideInInspector] public List<GridNode> hexgridNodes = new List<GridNode>();
    
    [SerializeField] private List<Tilemap> obstacleTileMaps = new List<Tilemap>();

    [SerializeField] private int hexgridXWidth;
    [SerializeField] private int hexgridYHeight;
    [SerializeField] private int hexgridZWdith;

    private Vector3 cellSize;
    private List<Vector3Int> hexgridNodeAroundOffsets = new List<Vector3Int>() { new Vector3Int(0, -1, 1), new Vector3Int(0, 1, -1), new Vector3Int(1, -1, 0), new Vector3Int(1, 0, -1), new Vector3Int(-1, 1, 0), new Vector3Int(-1, 0, 1) };

    private List<GridNode> openNodeList = new List<GridNode>();
    private List<GridNode> closedNodeList = new List<GridNode>();

    private void Awake()
    {
        cellSize = gridBase.cellSize;

        CreateNodes();
    }

    private void CreateNodes()
    {
        for (int x = -hexgridXWidth; x <= hexgridXWidth; x++)
        {
            for (int y = -hexgridYHeight; y <= hexgridYHeight; y++)
            {
                for (int z = -hexgridZWdith; z <= hexgridZWdith; z++)
                {
                    if (x + y + z != 0) continue;

                    Vector3Int hexgridPosition = new Vector3Int(x, y, z);
                    Vector3 worldgridPosition = HexgridToWorldgrid(hexgridPosition);
                    Vector3Int cellgridPosition = moveableTilemap.WorldToCell(worldgridPosition);
                    TileBase groundTile = moveableTilemap.GetTile(cellgridPosition);

                    if (groundTile != null)
                    {
                        bool isObstacle = false;

                        foreach (Tilemap obstacleTileMap in obstacleTileMaps)
                        {
                            TileBase obstacleTile = obstacleTileMap.GetTile(cellgridPosition);

                            if (obstacleTile != null)
                            {
                                isObstacle = true;
                                break;
                            }
                        }

                        hexgridNodes.Add(new GridNode(hexgridPosition, cellgridPosition, worldgridPosition, isObstacle));
                    }
                }
            }
        }
    }

    private void Initialize()
    {
        foreach (GridNode hexgridNode in hexgridNodes)
        {
            hexgridNode.gCost = 0;
            hexgridNode.hCost = int.MaxValue;
            hexgridNode.fCost = hexgridNode.gCost + hexgridNode.hCost;

            hexgridNode.cameFromNode = null;
        }
        openNodeList.Clear();
        closedNodeList.Clear();
    }

    private int GetHeuristicDistance(Vector3Int hexgridStartPosition, Vector3Int hexgridDestinationPosition)
    {
        return Mathf.Max(new int[] { Mathf.Abs(hexgridStartPosition.x - hexgridDestinationPosition.x), Mathf.Abs(hexgridStartPosition.y - hexgridDestinationPosition.y), Mathf.Abs(hexgridStartPosition.z - hexgridDestinationPosition.z) });
    }

    public List<GridNode> PathFinding(Vector3Int cellgridStartPosition, Vector3Int cellgridDestinationPosition)
    {
        Initialize();

        GridNode startNode = hexgridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridStartPosition);
        GridNode destinationNode = hexgridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridDestinationPosition);

        if (destinationNode == null) return null;

        startNode.cameFromNode = null;
        startNode.gCost = 0;
        startNode.hCost = GetHeuristicDistance(startNode.hexgridPosition, destinationNode.hexgridPosition);
        startNode.fCost = startNode.gCost + startNode.fCost;
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList = openNodeList.OrderBy(node => node.fCost).ThenByDescending(node => node.gCost).ToList();
            GridNode currentNode = openNodeList[0];
            openNodeList.Remove(currentNode);
            closedNodeList.Add(currentNode);

            int nextGCost = currentNode.gCost + 1;

            if (closedNodeList.Contains(destinationNode))
            {
                break;
            }
            
            foreach (Vector3Int hexgridNodeAroundOffset in hexgridNodeAroundOffsets)
            {
                GridNode adjacentNode = hexgridNodes.FirstOrDefault(node => node.hexgridPosition == currentNode.hexgridPosition + hexgridNodeAroundOffset);
                
                if (adjacentNode != null)
                {
                    if (!adjacentNode.isObstacle && !closedNodeList.Contains(adjacentNode))
                    {
                        if (!openNodeList.Contains(adjacentNode))
                        {
                            adjacentNode.cameFromNode = currentNode;
                            adjacentNode.gCost = nextGCost;
                            adjacentNode.hCost = GetHeuristicDistance(adjacentNode.hexgridPosition, destinationNode.hexgridPosition);
                            adjacentNode.fCost = adjacentNode.gCost + adjacentNode.hCost;
                            openNodeList.Add(adjacentNode);
                        }
                        else if (adjacentNode.fCost > nextGCost + adjacentNode.hCost)
                        {
                            adjacentNode.cameFromNode = currentNode;
                            adjacentNode.gCost = nextGCost;
                        }
                    }
                }
            }
        }

        List<GridNode> path = new List<GridNode>();
        if (closedNodeList.Contains(destinationNode))
        {
            GridNode currentNode= destinationNode;

            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Add(currentNode);
            path.Reverse();
        }
        return path;
    }

    public Vector3 HexgridToWorldgrid(Vector3Int hexgridPosition)
    {
        return new Vector3(hexgridPosition.x * 0.5f * cellSize.x - hexgridPosition.z * 0.5f * cellSize.x, hexgridPosition.y * 0.5f * cellSize.y - hexgridPosition.x * 0.25f * cellSize.y - hexgridPosition.z * 0.25f * cellSize.y);
    }

    public Vector3Int? HexgridToCellgrid(Vector3Int hexgridPosition)
    {
        // return new Vector3Int(Mathf.RoundToInt((hexgridPosition.x - hexgridPosition.z) / 2.0f), hexgridPosition.y);
        GridNode hexgridNode = hexgridNodes.FirstOrDefault(node => node.hexgridPosition == hexgridPosition);
        return hexgridNode == null ? null : hexgridNode.cellgridPosition;
        // return hexgridNodes.FirstOrDefault(node => node.hexgridPosition == hexgridPosition).cellgridPosition;
    }

    public Vector3Int CellgridToHexgrid(Vector3Int cellgridPosition)
    {
        return hexgridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridPosition).hexgridPosition;
    }

    private void OnDrawGizmos()
    {
        /*for (int x = scanStartX; x <= scanFinishX; x++)
        {
            for (int y = scanStartY; y <= scanFinishY; y++)
            {
                for (int z = scanStartZ; z <= scanFinishZ; z++)
                {
                    if (x + y + z != 0) continue;

                    // Draw Hexgrid
                }
            }
        }*/
    }
}
