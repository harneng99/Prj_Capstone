using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathInformation
{
    public List<GridNode> path { get; private set; } = null;
    public int requiredStamina { get; private set; } = int.MaxValue;

    public PathInformation(List<GridNode> path, int requiredStamina)
    {
        this.path = path;
        this.requiredStamina = requiredStamina;
    }
}

public class Pathfinder : MonoBehaviour
{
    public Grid gridBase { get; private set; }
    public Tilemap baseTilemap { get; private set; }
    public Tilemap moveableTilemap { get; private set; }
    public Tilemap objectTilemap { get; private set; }
    public List<GridNode> gridNodes { get; private set; } = new List<GridNode>();

    [SerializeField, EnumFlags] private MoveableTileLayer moveableLayerTypes;
    [SerializeField, EnumFlags] private ObjectTileLayer objectLayerTypes;

    [SerializeField, Tooltip("Height difference between the cells that the entity can route.")] private int routableLevelDifference = 1;
    [SerializeField] private int hexgridXWidth;
    [SerializeField] private int hexgridYHeight;
    [SerializeField] private int hexgridZWidth;

    private const float epsilon = 0.001f;
    private Vector3 cellSize;
    private List<Vector3Int> hexgridNodeAroundOffsets = new List<Vector3Int>() { new Vector3Int(0, -1, 1), new Vector3Int(0, 1, -1), new Vector3Int(1, -1, 0), new Vector3Int(1, 0, -1), new Vector3Int(-1, 1, 0), new Vector3Int(-1, 0, 1) };

    private List<GridNode> openNodeList = new List<GridNode>();
    private List<GridNode> closedNodeList = new List<GridNode>();

    private void Awake()
    {
        gridBase = FindAnyObjectByType<Grid>();
        cellSize = gridBase.cellSize;

        moveableTilemap = GameObject.FindWithTag("MoveableTilemap").GetComponent<Tilemap>();
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();

        CreateNodes();
    }

    private void CreateNodes()
    {
        for (int x = -hexgridXWidth; x <= hexgridXWidth; x++)
        {
            for (int y = -hexgridYHeight; y <= hexgridYHeight; y++)
            {
                for (int z = -hexgridZWidth; z <= hexgridZWidth; z++)
                {
                    if (x + y + z != 0) continue;

                    Vector3Int hexgridPosition = new Vector3Int(x, y, z);
                    Vector3 worldgridPosition = HexgridToWorldgrid(hexgridPosition);
                    Vector3Int cellgridPosition = moveableTilemap.WorldToCell(worldgridPosition);
                    
                    GameObject tileGameObject = moveableTilemap.GetInstantiatedObject(cellgridPosition);

                    if (tileGameObject == null) continue;

                    CustomTileData customTileData = tileGameObject.GetComponent<CustomTileData>();
                    // TileBase moveableTile = moveableTilemap.GetTile(cellgridPosition);

                    if (moveableLayerTypes.HasFlag(customTileData.moveableTileLayer))
                    {
                        gridNodes.Add(new GridNode(hexgridPosition, cellgridPosition, worldgridPosition, IsObstacle(cellgridPosition), customTileData));
                    }
                }
            }
        }
    }

    private void Initialize()
    {
        foreach (GridNode hexgridNode in gridNodes)
        {
            hexgridNode.gCost = 0;
            hexgridNode.hCost = int.MaxValue;
            hexgridNode.fCost = hexgridNode.gCost + hexgridNode.hCost;

            hexgridNode.cameFromNode = null;
        }
        openNodeList.Clear();
        closedNodeList.Clear();
    }

    public int GetHeuristicDistance(Vector3Int hexgridStartPosition, Vector3Int hexgridDestinationPosition)
    {
        return Mathf.Max(new int[] { Mathf.Abs(hexgridStartPosition.x - hexgridDestinationPosition.x), Mathf.Abs(hexgridStartPosition.y - hexgridDestinationPosition.y), Mathf.Abs(hexgridStartPosition.z - hexgridDestinationPosition.z) });
    }

    public PathInformation PathFinding(Vector3Int cellgridStartPosition, Vector3Int cellgridDestinationPosition)
    {
        Initialize();

        GridNode startNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridStartPosition);
        GridNode destinationNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridDestinationPosition);

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
                GridNode adjacentNode = gridNodes.FirstOrDefault(node => node.hexgridPosition == currentNode.hexgridPosition + hexgridNodeAroundOffset);
                
                if (adjacentNode != null) // if the node exists
                {
                    if (!adjacentNode.isObstacle && !closedNodeList.Contains(adjacentNode)) // if the entity can move on that node
                    {
                        if (Mathf.Abs(adjacentNode.customTileData.tileLevel - currentNode.customTileData.tileLevel) <= routableLevelDifference) // if the height condition meets
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
        }

        if (closedNodeList.Contains(destinationNode))
        {
            int requiredStamina = 0;
            List<GridNode> path = new List<GridNode>();

            GridNode prevNode = null;
            GridNode currentNode = destinationNode;

            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode);

                prevNode = currentNode;
                currentNode = currentNode.cameFromNode;

                requiredStamina += CalculateStamina(currentNode, prevNode);
            }

            path.Add(currentNode);
            path.Reverse();
            return new PathInformation(path, requiredStamina);
        }

        return null;
    }

    private int CalculateStamina(GridNode prevNode, GridNode nextNode)
    {
        int requiredStamina = 0;
        int levelDifference = nextNode.customTileData.tileLevel - prevNode.customTileData.tileLevel;

        if (levelDifference >= 1)
        {
            requiredStamina += Mathf.Abs(levelDifference);
        }
        else
        {
            requiredStamina += 1;
        }

        return requiredStamina;
    }

    public Vector3 HexgridToWorldgrid(Vector3Int hexgridPosition)
    {
        return new Vector3(hexgridPosition.x * 0.5f * cellSize.x - hexgridPosition.z * 0.5f * cellSize.x, hexgridPosition.y * 0.5f * cellSize.y - hexgridPosition.x * 0.25f * cellSize.y - hexgridPosition.z * 0.25f * cellSize.y);
    }

    public Vector3Int HexgridToCellgrid(Vector3Int hexgridPosition)
    {
        // RoundToInt is not consistent due to the fundamental problem of floating point expression
        return new Vector3Int(Mathf.RoundToInt((hexgridPosition.x - hexgridPosition.z) / 2.0f - epsilon), hexgridPosition.y);
        
        // GridNode hexgridNode = gridNodes.FirstOrDefault(node => node.hexgridPosition == hexgridPosition);
        // return hexgridNode == null ? null : hexgridNode.cellgridPosition;
    }

    public Vector3Int CellgridToHexgrid(Vector3Int cellgridPosition)
    {
        return new Vector3Int(cellgridPosition.x - cellgridPosition.y / 2, cellgridPosition.y, -cellgridPosition.x - cellgridPosition.y / 2 - cellgridPosition.y % 2);
        
        // GridNode cellgridNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridPosition);
        // return cellgridNode == null ? null : cellgridNode.cellgridPosition;
    }

    /// <summary>
    /// Checks whether there is an obstacle at given cell grid position.
    /// </summary>
    /// <param name="cellgridPosition"></param>
    /// <returns></returns>
    public bool IsObstacle(Vector3Int cellgridPosition)
    {
        TileBase obstacleTile = objectTilemap.GetTile(cellgridPosition);

        if (obstacleTile == null)
        {
            return false;
        }
        else
        {
            GameObject tileGameObject = objectTilemap.GetInstantiatedObject(cellgridPosition);
            CustomTileData customTileData = tileGameObject.GetComponent<CustomTileData>();
            return objectLayerTypes.HasFlag(customTileData.objectTileLayer);
        }
    }

    public bool isMoveable(Vector3Int cellgridPosition)
    {
        TileBase moveableTile = moveableTilemap.GetTile(cellgridPosition);

        if (moveableTile == null)
        {
            return false;
        }
        else
        {
            GameObject tileGameObject = moveableTilemap.GetInstantiatedObject(cellgridPosition);
            CustomTileData customTileData = tileGameObject.GetComponent<CustomTileData>();
            return moveableLayerTypes.HasFlag(customTileData.moveableTileLayer);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 horizontalOffset = new Vector3(cellSize.x / 4, 0.0f, 0.0f);
        Vector3 verticalOffset = new Vector3(0.0f, cellSize.y / 8, 0.0f);
        Gizmos.color = Color.green;

        for (int x = -hexgridXWidth; x <= hexgridXWidth; x++)
        {
            for (int y = -hexgridYHeight; y <= hexgridYHeight; y++)
            {
                for (int z = -hexgridZWidth; z <= hexgridZWidth; z++)
                {
                    if (x + y + z != 0) continue;

                    // Draw Hexgrid
                    Vector3Int currentHexgridPosition = new Vector3Int(x, y, z);
                    Vector3 currentWorldgridPosition = HexgridToWorldgrid(currentHexgridPosition);
                    Vector3Int currentCellgridPosition = HexgridToCellgrid(currentHexgridPosition);
                    Vector3 worldgridTopPosition = currentWorldgridPosition + new Vector3(0.0f, cellSize.y / 2);
                    Vector3 worldgridTopLeftPosition = currentWorldgridPosition - new Vector3(cellSize.x / 2, -cellSize.y / 4);
                    Vector3 worldgridTopRightPosition = currentWorldgridPosition + new Vector3(cellSize.x / 2, cellSize.y / 4);
                    Vector3 worldgridBottomPosition = currentWorldgridPosition - new Vector3(0.0f, cellSize.y / 2);
                    Vector3 worldgridBottomLeftPosition = currentWorldgridPosition - new Vector3(cellSize.x / 2, cellSize.y / 4);
                    Vector3 worldgridBottomRightPosition = currentWorldgridPosition + new Vector3(cellSize.x / 2, -cellSize.y / 4);

                    Handles.Label(currentWorldgridPosition - horizontalOffset + verticalOffset, currentHexgridPosition.ToString());
                    Handles.Label(currentWorldgridPosition - horizontalOffset - verticalOffset, new Vector2Int(currentCellgridPosition.x, currentCellgridPosition.y).ToString());

                    Gizmos.DrawLine(worldgridTopPosition, worldgridTopRightPosition);
                    Gizmos.DrawLine(worldgridTopRightPosition, worldgridBottomRightPosition);
                    Gizmos.DrawLine(worldgridBottomRightPosition, worldgridBottomPosition);
                    Gizmos.DrawLine(worldgridBottomPosition, worldgridBottomLeftPosition);
                    Gizmos.DrawLine(worldgridBottomLeftPosition, worldgridTopLeftPosition);
                    Gizmos.DrawLine(worldgridTopLeftPosition, worldgridTopPosition);
                }
            }
        }
    }
}
