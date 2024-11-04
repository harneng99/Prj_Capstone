using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

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
    private List<Vector3Int> cellgridNodeAroundOffsets = new List<Vector3Int>() { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0), new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) };

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
        /*for (int x = -hexgridXWidth; x <= hexgridXWidth; x++)
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
        }*/

        BoundsInt bounds = moveableTilemap.cellBounds;

        for (int x = -bounds.size.x; x <= bounds.size.x; x++)
        {
            for (int y = -bounds.size.y; y <= bounds.size.y; y++)
            {
                Vector3Int cellgridPosition = new Vector3Int(x, y, 0);
                Vector3 worldgridPosition = new Vector3(x + 0.5f, y + 0.5f, 0.0f);

                GameObject tileGameObject = moveableTilemap.GetInstantiatedObject(cellgridPosition);

                if (tileGameObject == null) continue;

                CustomTileData customTileData = tileGameObject.GetComponent<CustomTileData>();

                if (moveableLayerTypes.HasFlag(customTileData.moveableTileLayer))
                {
                    gridNodes.Add(new GridNode(Vector3Int.zero, cellgridPosition, worldgridPosition, IsObstacle(cellgridPosition), customTileData));
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

    public float GetHeuristicDistance(Vector3Int hexgridStartPosition, Vector3Int hexgridDestinationPosition)
    {
        // return Mathf.Max(new int[] { Mathf.Abs(hexgridStartPosition.x - hexgridDestinationPosition.x), Mathf.Abs(hexgridStartPosition.y - hexgridDestinationPosition.y), Mathf.Abs(hexgridStartPosition.z - hexgridDestinationPosition.z) });

        return Vector3Int.Distance(hexgridStartPosition, hexgridDestinationPosition);
    }

    public PathInformation PathFinding(Vector3Int cellgridStartPosition, Vector3Int cellgridDestinationPosition, bool allowOverlap = true)
    {
        Initialize();

        GridNode startNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridStartPosition);
        GridNode destinationNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == cellgridDestinationPosition);


        if (allowOverlap == false && Manager.Instance.gameManager.EntityExistsAt(cellgridDestinationPosition) != null) return null;
        if (startNode == null || destinationNode == null) return null;

        startNode.cameFromNode = null;
        startNode.gCost = 0.0f;
        startNode.hCost = GetHeuristicDistance(startNode.hexgridPosition, destinationNode.hexgridPosition);
        startNode.fCost = startNode.gCost + startNode.fCost;
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList = openNodeList.OrderBy(node => node.fCost).ThenByDescending(node => node.gCost).ToList();
            GridNode currentNode = openNodeList[0];
            openNodeList.Remove(currentNode);
            closedNodeList.Add(currentNode);

            float nextGCost = currentNode.gCost + 1;

            if (closedNodeList.Contains(destinationNode))
            {
                break;
            }
            
            foreach (Vector3Int cellgridNodeAroundOffset in cellgridNodeAroundOffsets)
            {
                GridNode adjacentNode = gridNodes.FirstOrDefault(node => node.cellgridPosition == currentNode.cellgridPosition + cellgridNodeAroundOffset);
                
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
                                adjacentNode.hCost = GetHeuristicDistance(adjacentNode.cellgridPosition, destinationNode.cellgridPosition);
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
        if (cellgridPosition.y < 0 && cellgridPosition.y % 2 != 0)
        {
            return new Vector3Int(cellgridPosition.x - cellgridPosition.y / 2, cellgridPosition.y, -cellgridPosition.x - cellgridPosition.y / 2 - cellgridPosition.y % 2) + new Vector3Int(1, 0, -1);
        }
        else
        {
            return new Vector3Int(cellgridPosition.x - cellgridPosition.y / 2, cellgridPosition.y, -cellgridPosition.x - cellgridPosition.y / 2 - cellgridPosition.y % 2);
        }
        
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

    public bool IsMoveable(Vector3Int cellgridPosition)
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
}
