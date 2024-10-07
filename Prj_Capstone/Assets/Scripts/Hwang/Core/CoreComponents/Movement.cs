using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum GridType { Hexgrid, Cellgrid }

[RequireComponent(typeof(Pathfinder))]
public abstract class Movement : CoreComponent
{
    [SerializeField] protected TileBase moveRangeHighlightedTileBase;
    [SerializeField] private Vector3Int moveRangeInHexGrid;
    [SerializeField] private int maxMovementStamina;

    public Pathfinder pathfinder { get; private set; }

    public Vector3? prevWorldgridPosition { get; set; }
    public Vector3Int currentCellgridPosition { get; private set; }
    public Vector3Int currentHexgridPosition { get; private set; }
    
    // public Tilemap highlightedTilemap { get; private set; }
    
    protected bool isMoving;
    protected bool isShowingMoveableTiles;
    protected Coroutine smoothMovementCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        pathfinder = GetComponent<Pathfinder>();

        // highlightedTilemap = GameObject.FindWithTag("HighlightedTilemap").GetComponent<Tilemap>();
        entity.onPointerClick += () => { ShowMoveableTiles(!isShowingMoveableTiles); };
    }

    private void Start()
    {
        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();

        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        entity.transform.position = pathfinder.HexgridToWorldgrid(currentHexgridPosition) + Vector3.up * entity.entityCollider.bounds.extents.y;
    }

    // This is called whenever the mouse clicks anywhere. It is different from OnPointerClick event function.
    protected virtual void MouseLeftClick()
    {

    }

    public void UpdateGridPositionData()
    {
        prevWorldgridPosition = entity.GetEntityFeetPosition();
        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        // entity.SetEntityFeetPosition(pathfinder.HexgridToWorldgrid(currentHexgridPosition));
        // entity.transform.position = pathfinder.HexgridToWorldgrid(currentHexgridPosition) + Vector3.up * entity.entityCollider.bounds.extents.y;
    }

    /// <summary>
    /// Gets the destination world grid position and move the entity. Returns whether the movement succeeded or not.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="instantMove"></param>
    /// <returns></returns>
    public bool MoveToGrid(Vector3 destinationWorldgridPosition, bool instantMove)
    {
        Vector3Int destinationCellgridPosition = pathfinder.moveableTilemap.WorldToCell(destinationWorldgridPosition);

        // if (Manager.Instance.gameManager.fogTilemap.HasTile(destinationCellgridPosition)) return false;
        
        if (instantMove)
        {
            GameObject tileGameObject = pathfinder.moveableTilemap.GetInstantiatedObject(destinationCellgridPosition);

            if (tileGameObject != null)
            {
                if (!pathfinder.IsObstacle(destinationCellgridPosition) && pathfinder.isMoveable(destinationCellgridPosition))
                {
                    entity.SetEntityFeetPosition(pathfinder.moveableTilemap.CellToWorld(destinationCellgridPosition));
                    UpdateGridPositionData();
                    return true;
                }
            }

            return false;
        }
        else
        {
            isMoving = true;
            List<GridNode> path = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);
            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(path));
            return true;
        }
    }

    /// <summary>
    /// Gets the destination hex/cell grid position and move the entity. Returns whether the movement succeeded or not.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="gridType"></param>
    /// <param name="instantMove"></param>
    /// <returns></returns>
    public bool MoveToGrid(Vector3Int destinationGridPosition, GridType gridType, bool instantMove)
    {
        Vector3Int destinationCellgridPosition = gridType.Equals(GridType.Hexgrid) ? pathfinder.HexgridToCellgrid(destinationGridPosition) : destinationGridPosition;

        // if (Manager.Instance.gameManager.fogTilemap.HasTile(destinationCellgridPosition)) return false;

        if (instantMove)
        {
            GameObject tileGameObject = pathfinder.moveableTilemap.GetInstantiatedObject(destinationCellgridPosition);

            if (tileGameObject != null)
            {
                if (!pathfinder.IsObstacle(destinationCellgridPosition) && pathfinder.isMoveable(destinationCellgridPosition))
                {
                    entity.SetEntityFeetPosition(pathfinder.moveableTilemap.CellToWorld(destinationCellgridPosition));
                    UpdateGridPositionData();
                    return true;
                }
            }

            return false;
        }
        else
        {
            isMoving = true;
            List<GridNode> path = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);
            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(path));
            return true;
        }
    }

    private IEnumerator MoveEntitySmooth(List<GridNode> path)
    {
        int requiredStamina = 0;
        GridNode prevNode = path.First();
        GridNode destinationNode = path.Last();

        path.RemoveAt(0);
        if (path.Count <= 0) yield break;
        GridNode currentDestinationNode = path.First();

        while (Vector3.Distance(entity.GetEntityFeetPosition(), destinationNode.worldgridPosition) > epsilon)
        {
            if (Vector3.Distance(entity.GetEntityFeetPosition(), currentDestinationNode.worldgridPosition) < epsilon)
            {
                if (path.Count > 0)
                {
                    path.RemoveAt(0);
                    
                    if (path.Count > 0)
                    {
                        int levelDifference = currentDestinationNode.customTileData.tileLevel - prevNode.customTileData.tileLevel;

                        if (levelDifference >= 1)
                        {
                            requiredStamina += Mathf.Abs(levelDifference);
                        }
                        else
                        {
                            requiredStamina += 1;
                        }

                        prevNode = currentDestinationNode;
                        currentDestinationNode = path.First();
                    }
                }
                else
                {
                    isMoving = false;
                    UpdateGridPositionData();
                    break;
                }
            }
            else
            {
                entity.transform.position = Vector3.MoveTowards(entity.transform.position, currentDestinationNode.worldgridPosition + Vector3.up * entity.entityCollider.bounds.extents.y, entity.entityConsistentData.movementVelocity * Time.deltaTime);
            }

            yield return null;
        }

        entity.entityStat.stamina.DecreaseCurrentValue(requiredStamina);
        isMoving = false;
    }

    /// <summary>
    /// Gets whether entity is going to show moveable tile area in bool value. True means it will show its moveable tile area, and vice versa.
    /// </summary>
    /// <param name="showTile"></param>
    public virtual void ShowMoveableTiles(bool showTile)
    {
        if (!isMoving)
        {
            entity.highlightedTilemap.ClearAllTiles();
            isShowingMoveableTiles = false;

            if (!showTile) return;

            for (int x = -moveRangeInHexGrid.x; x <= moveRangeInHexGrid.x; x++)
            {
                for (int y = -moveRangeInHexGrid.y; y <= moveRangeInHexGrid.y; y++)
                {
                    for (int z = -moveRangeInHexGrid.z; z <= moveRangeInHexGrid.z; z++)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int moveableHexgridPosition = currentHexgridPosition + new Vector3Int(x, y, z);
                        Vector3Int moveableCellgridPosition = pathfinder.HexgridToCellgrid(moveableHexgridPosition);

                        GridNode moveableGridNode = pathfinder.gridNodes.FirstOrDefault(node => node.cellgridPosition == moveableCellgridPosition);

                        if (moveableGridNode == null) continue;

                        if (!moveableGridNode.isObstacle)
                        {
                            List<GridNode> path = pathfinder.PathFinding(currentCellgridPosition, moveableCellgridPosition);

                            int requiredStamina = 0;
                            bool pathOutOfRange = false;
                            GridNode prevGridNode = null;

                            foreach (GridNode gridNode in path)
                            {
                                if ((Mathf.Abs(gridNode.hexgridPosition.x - currentHexgridPosition.x) > moveRangeInHexGrid.x) || (Mathf.Abs(gridNode.hexgridPosition.y - currentHexgridPosition.y) > moveRangeInHexGrid.y) || (Mathf.Abs(gridNode.hexgridPosition.z - currentHexgridPosition.z) > moveRangeInHexGrid.z))
                                {
                                    pathOutOfRange = true;
                                    break;
                                }

                                if (prevGridNode != null)
                                {
                                    int levelDifference = gridNode.customTileData.tileLevel - prevGridNode.customTileData.tileLevel;

                                    if (levelDifference >= 1)
                                    {
                                        requiredStamina += Mathf.Abs(levelDifference);
                                    }
                                    else
                                    {
                                        requiredStamina += 1;
                                    }
                                }

                                if (requiredStamina > maxMovementStamina)
                                {
                                    pathOutOfRange = true;
                                    break;
                                }

                                prevGridNode = gridNode;
                            }

                            if (!pathOutOfRange)
                            {
                                entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                            }
                        }
                    }
                }
            }

            isShowingMoveableTiles = true;
        }
    }
}
