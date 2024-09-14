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
    [field: SerializeField] public Tilemap moveableTileMap { get; private set; }
    
    [HideInInspector] public List<HexgridNode> hexgridNodes = new List<HexgridNode>();
    
    [SerializeField] private List<Tilemap> obstacleTileMaps = new List<Tilemap>();

    [SerializeField] private int hexgridXWidth;
    [SerializeField] private int hexgridYHeight;
    [SerializeField] private int hexgridZWdith;

    private Vector3 cellSize;
    private List<Vector3Int> hexgridNodeAroundOffsets = new List<Vector3Int>() { new Vector3Int(0, -1, 1), new Vector3Int(0, 1, -1), new Vector3Int(1, -1, 0), new Vector3Int(1, 0, -1), new Vector3Int(-1, 1, 0), new Vector3Int(-1, 0, 1) };

    private List<HexgridNode> openNodeList = new List<HexgridNode>();
    private List<HexgridNode> closedNodeList = new List<HexgridNode>();

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
                    Vector3Int cellgridPosition = moveableTileMap.WorldToCell(HexgridToWorldgrid(hexgridPosition));
                    TileBase groundTile = moveableTileMap.GetTile(cellgridPosition);

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

                        hexgridNodes.Add(new HexgridNode(hexgridPosition, cellgridPosition, isObstacle));
                    }
                }
            }
        }
    }

    private void Initialize()
    {
        foreach (HexgridNode hexgridNode in hexgridNodes)
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

    public List<Vector3> PathFinding(Vector3Int cellgridStartPosition, Vector3Int cellgridDestinationPosition)
    {
        Initialize();

        HexgridNode startNode = hexgridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridStartPosition);
        HexgridNode destinationNode = hexgridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridDestinationPosition);

        if (destinationNode == null) return null;

        startNode.cameFromNode = null;
        startNode.gCost = 0;
        startNode.hCost = GetHeuristicDistance(startNode.hexgridPosition, destinationNode.hexgridPosition);
        startNode.fCost = startNode.gCost + startNode.fCost;
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList = openNodeList.OrderBy(node => node.fCost).ThenByDescending(node => node.gCost).ToList();
            HexgridNode currentNode = openNodeList[0];
            openNodeList.Remove(currentNode);
            closedNodeList.Add(currentNode);

            int nextGCost = currentNode.gCost + 1;

            if (closedNodeList.Contains(destinationNode))
            {
                break;
            }
            
            foreach (Vector3Int hexgridNodeAroundOffset in hexgridNodeAroundOffsets)
            {
                HexgridNode adjacentNode = hexgridNodes.FirstOrDefault(node => node.hexgridPosition == currentNode.hexgridPosition + hexgridNodeAroundOffset);
                
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

        if (closedNodeList.Contains(destinationNode))
        {
            List<Vector3> path = new List<Vector3>();

            HexgridNode currentNode = destinationNode;

            while (currentNode.cameFromNode != null)
            {
                path.Add(HexgridToWorldgrid(currentNode.hexgridPosition));
                currentNode = currentNode.cameFromNode;
            }
            path.Add(HexgridToWorldgrid(currentNode.hexgridPosition));
            path.Reverse();

            return path;
        }
        else return null;
    }

    public Vector3 HexgridToWorldgrid(Vector3Int hexgridPosition)
    {
        return new Vector3(hexgridPosition.x * 0.5f * cellSize.x - hexgridPosition.z * 0.5f * cellSize.x, hexgridPosition.y * 0.5f * cellSize.y - hexgridPosition.x * 0.25f * cellSize.y - hexgridPosition.z * 0.25f * cellSize.y);
    }

    public Vector3Int? HexgridToCellgrid(Vector3Int hexgridPosition)
    {
        // return new Vector3Int(Mathf.RoundToInt((hexgridPosition.x - hexgridPosition.z) / 2.0f), hexgridPosition.y);
        HexgridNode hexgridNode = hexgridNodes.FirstOrDefault(node => node.hexgridPosition == hexgridPosition);
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
