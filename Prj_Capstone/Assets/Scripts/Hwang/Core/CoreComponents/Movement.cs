using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum GridType { Hexgrid, Cellgrid }

[RequireComponent(typeof(Pathfinder))]
public class Movement : CoreComponent
{
    [field: SerializeField] public Vector3Int moveRangeInHexGrid { get; protected set; }
    [SerializeField] protected TileBase moveRangeHighlightedTileBase;
    [SerializeField] protected int maxMovementStamina;

    public Pathfinder pathfinder { get; private set; }
    public Vector3? currentWorldgridPosition { get; set; }
    public Vector3Int currentCellgridPosition { get; private set; }
    public Vector3Int currentHexgridPosition { get; private set; }

    public event Action smoothMoveFinished;
    
    public bool isMoving { get; protected set; }
    protected Coroutine smoothMovementCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        pathfinder = GetComponent<Pathfinder>();
    }

    protected virtual void Start()
    {
        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        entity.transform.position = pathfinder.moveableTilemap.CellToWorld(currentCellgridPosition) + Vector3.up * entity.entityCollider.bounds.extents.y;
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (!Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection)
            {
                ToggleMoveableTilemap(UtilityFunctions.IsTilemapEmpty(entity.highlightedTilemap));
            }
        }
        else if (eventData.button.Equals(PointerEventData.InputButton.Right))
        {
            ToggleMoveableTilemap(false);
        }
    }

    public void UpdateGridPositionData()
    {
        currentWorldgridPosition = entity.GetEntityFeetPosition();
        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
    }

    /// <summary>
    /// Gets the destination world grid position and move the entity. Returns whether the movement succeeded or not.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="instantMove"></param>
    /// <returns></returns>
    public virtual bool MoveToGrid(Vector3 destinationWorldgridPosition, bool instantMove)
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
            PathInformation pathInformation = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);

            if (pathInformation.requiredStamina > entity.entityStat.stamina.currentValue)
            {
                Manager.Instance.uiManager.ShowWarningUI("Warning: Not enough stamina.");
                return false;
            }

            isMoving = true;
            entity.entityStat.stamina.DecreaseCurrentValue(pathInformation.requiredStamina);
            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(pathInformation.path));
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
    public virtual bool MoveToGrid(Vector3Int destinationGridPosition, GridType gridType, bool instantMove)
    {
        Vector3Int destinationCellgridPosition = gridType.Equals(GridType.Hexgrid) ? pathfinder.HexgridToCellgrid(destinationGridPosition) : destinationGridPosition;

        // if (Manager.Instance.gameManager.fogTilemap.HasTile(destinationCellgridPosition)) return false;
        if (Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition)) return false;
        if (currentCellgridPosition.Equals(destinationCellgridPosition)) return false;

        if (instantMove)
        {
            GameObject tileGameObject = pathfinder.moveableTilemap.GetInstantiatedObject(destinationCellgridPosition);

            if (tileGameObject != null)
            {
                if (!pathfinder.IsObstacle(destinationCellgridPosition) && pathfinder.isMoveable(destinationCellgridPosition))
                {
                    entity.SetEntityFeetPosition(pathfinder.moveableTilemap.CellToWorld(destinationCellgridPosition));
                    ToggleMoveableTilemap(false);
                    UpdateGridPositionData();
                    return true;
                }
            }

            return false;
        }
        else
        {
            PathInformation pathInformation = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);

            if (pathInformation.requiredStamina > entity.entityStat.stamina.currentValue)
            {
                Debug.LogWarning(entity.name + " is trying to move more than current stamina.");
                return false;
            }

            isMoving = true;
            entity.entityStat.stamina.DecreaseCurrentValue(pathInformation.requiredStamina);
            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(pathInformation.path));
            return true;
        }
    }

    protected IEnumerator MoveEntitySmooth(List<GridNode> path)
    {
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

        UpdateGridPositionData();
        smoothMoveFinished?.Invoke();
        isMoving = false;
    }

    /// <summary>
    /// Gets whether entity is going to show moveable tile area in bool value. True means it will show its moveable tile area, and vice versa.
    /// </summary>
    /// <param name="showTile"></param>
    public void ToggleMoveableTilemap(bool showTile = true)
    {
        entity.highlightedTilemap.ClearAllTiles();

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

                    if (moveableGridNode != null && !moveableGridNode.isObstacle)
                    {
                        PathInformation pathInformation = pathfinder.PathFinding(currentCellgridPosition, moveableCellgridPosition);

                        if (pathInformation == null || entity.entityStat.stamina.currentValue < pathInformation.requiredStamina) continue;
                        
                        if (!PathOutOfRange(currentHexgridPosition, pathInformation))
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                        }
                    }
                }
            }
        }
    }

    public bool PathOutOfRange(Vector3Int currentHexgridPosition, PathInformation pathInformation)
    {
        foreach (GridNode gridNode in pathInformation.path)
        {
            if ((Mathf.Abs(gridNode.hexgridPosition.x - currentHexgridPosition.x) > moveRangeInHexGrid.x) || (Mathf.Abs(gridNode.hexgridPosition.y - currentHexgridPosition.y) > moveRangeInHexGrid.y) || (Mathf.Abs(gridNode.hexgridPosition.z - currentHexgridPosition.z) > moveRangeInHexGrid.z))
            {
                return true;
            }
        }

        return false;
    }
}
