using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum GridType { Hexgrid, Cellgrid }
public enum PieceType { Pawn, Knight, Bishop, Rook, Queen }

[RequireComponent(typeof(Pathfinder))]
public abstract class Movement : CoreComponent
{
    [field: SerializeField] public Vector3Int moveRangeInHexGrid { get; protected set; }
    [field: SerializeField] public PieceType pieceType { get; protected set; }
    [SerializeField] protected TileBase moveRangeHighlightedTileBase;
    [SerializeField] protected TileBase attackRangeHighlightedTileBase;
    [SerializeField] protected TileBase wallTileBase;
    [SerializeField] protected TileBase swampTileBase;
    [SerializeField] protected int maxMovementStamina;

    [Header("Knight Animation")]
    [SerializeField] protected float duration;
    [SerializeField] protected float destinationAlpha;
    [SerializeField] protected Vector3 destinationOffset;
    [SerializeField] protected Vector3 destinationRotation;

    public Pathfinder pathfinder { get; private set; }
    public Vector3? currentWorldgridPosition { get; set; }
    public Vector3Int currentCellgridPosition { get; private set; }
    public Vector3Int currentHexgridPosition { get; private set; }

    public event Action smoothMoveFinished;
    
    public bool isMoving { get; protected set; }
    protected Coroutine smoothMovementCoroutine;

    protected Vector3Int[] knightMovements = { new Vector3Int(-1, 2, 0), new Vector3Int(1, 2, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, -1, 0), new Vector3Int(1, -2, 0), new Vector3Int(-1, -2, 0), new Vector3Int(-2, 1, 0), new Vector3Int(-2, -1, 0) };
    protected Vector3Int[] bishopDirections = { new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) };

    protected override void Awake()
    {
        base.Awake();
        
        pathfinder = GetComponent<Pathfinder>();
    }

    protected virtual void Start()
    {
        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        // entity.transform.position = pathfinder.moveableTilemap.CellToWorld(currentCellgridPosition);
        entity.SetEntityPosition(currentCellgridPosition);
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            /*if (!Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection)
            {
                if (!Manager.Instance.gameManager.currentSelectedEntity.Equals(entity))
                {
                    DrawMoveableTilemap(UtilityFunctions.IsTilemapEmpty(entity.highlightedTilemap));
                }
                else
                {
                    DrawMoveableTilemap(false);
                    DrawMoveableTilemap(true);
                }
            }*/
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                if (Manager.Instance.gameManager.currentSelectedEntity.Equals(entity))
                {
                    DrawMoveableTilemap(UtilityFunctions.IsTilemapEmpty(entity.highlightedTilemap));
                }
                else
                {
                    DrawMoveableTilemap(false);
                    DrawMoveableTilemap(true);
                }
            }
        }
        else if (eventData.button.Equals(PointerEventData.InputButton.Right))
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                DrawMoveableTilemap(false);
            }
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
    public bool MoveToGrid(Vector3 destinationWorldgridPosition, bool instantMove)
    {
        Vector3Int destinationCellgridPosition = pathfinder.moveableTilemap.WorldToCell(destinationWorldgridPosition);

        return MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, instantMove);
    }

    public bool MoveToGrid(Vector3Int destinationGridPosition, GridType gridType, bool instantMove)
    {
        Vector3Int destinationCellgridPosition = gridType.Equals(GridType.Hexgrid) ? pathfinder.HexgridToCellgrid(destinationGridPosition) : destinationGridPosition;

        // if (Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition)) return false;
        if (currentCellgridPosition.Equals(destinationCellgridPosition)) return false;

        if (instantMove)
        {
            GameObject tileGameObject = pathfinder.moveableTilemap.GetInstantiatedObject(destinationCellgridPosition);

            if (tileGameObject != null)
            {
                if (!pathfinder.IsObstacle(destinationCellgridPosition) && pathfinder.IsMoveable(destinationCellgridPosition))
                {
                    entity.SetEntityPosition(destinationCellgridPosition);
                    DrawMoveableTilemap(false);
                    UpdateGridPositionData();
                    return true;
                }
            }

            return false;
        }
        else
        {
            PathInformation pathInformation = pathfinder.PathFinding(currentCellgridPosition, destinationCellgridPosition);

            /*if (pathInformation.requiredStamina > entity.entityStat.stamina.currentValue)
            {
                Debug.LogWarning(entity.name + " is trying to move more than current stamina.");
                return false;
            }*/

            isMoving = true;
            // entity.entityStat.stamina.DecreaseCurrentValue(pathInformation.requiredStamina);

            if (entity.isSelected)
            {
                Manager.Instance.uiManager.SetInformationUI(entity, entity.entityDescription, currentCellgridPosition);
            }

            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(pathInformation.path));
            return true;
        }
    }

    protected virtual IEnumerator MoveEntitySmooth(List<GridNode> path)
    {
        GridNode startNode = path.First();
        GridNode destinationNode = path.Last();

        path.RemoveAt(0);
        if (path.Count <= 0) yield break;

        if (pieceType != PieceType.Knight)
        {
            GridNode currentDestinationNode = path.First();

            while (Vector3.Distance(entity.transform.position, destinationNode.worldgridPosition) > epsilon)
            {
                if (Vector3.Distance(entity.transform.position, currentDestinationNode.worldgridPosition) < epsilon)
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
                    entity.transform.position = Vector3.MoveTowards(entity.transform.position, currentDestinationNode.worldgridPosition, entity.entityConsistentData.movementVelocity * Time.deltaTime);
                }

                yield return null;
            }
        }
        else
        {
            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(1.0f, destinationAlpha, elapsedTime / duration);
                entity.spriteRenderer.color = new Color(entity.spriteRenderer.color.r, entity.spriteRenderer.color.g, entity.spriteRenderer.color.b, newAlpha);
                // entity.transform.position = Vector3.MoveTowards(startNode.worldgridPosition, startNode.worldgridPosition + destinationOffset, Vector3.Length(destinationOffset) / duration);
                entity.transform.position = Vector3.Lerp(startNode.worldgridPosition, startNode.worldgridPosition + destinationOffset, elapsedTime / duration);
                entity.transform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(destinationRotation), elapsedTime / duration);
                yield return null;
            }
            entity.spriteRenderer.color = new Color(entity.spriteRenderer.color.r, entity.spriteRenderer.color.g, entity.spriteRenderer.color.b, 0.0f);

            elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(destinationAlpha, 1.0f, elapsedTime / duration);
                entity.spriteRenderer.color = new Color(entity.spriteRenderer.color.r, entity.spriteRenderer.color.g, entity.spriteRenderer.color.b, newAlpha);
                entity.transform.position = Vector3.Lerp(destinationNode.worldgridPosition + destinationOffset, destinationNode.worldgridPosition, elapsedTime / duration);
                entity.transform.rotation = Quaternion.Lerp(Quaternion.Euler(destinationOffset), Quaternion.identity, elapsedTime / duration);
                yield return null;
            }
        }

        UpdateGridPositionData();
        if (entity.isSelected)
        {
            Manager.Instance.uiManager.SetInformationUI(entity, entity.entityDescription, currentCellgridPosition);
        }
        smoothMoveFinished?.Invoke();
        isMoving = false;
    }

    /// <summary>
    /// Gets whether entity is going to show moveable tile area in bool value. True means it will show its moveable tile area, and vice versa.
    /// </summary>
    /// <param name="showTile"></param>
    public abstract void DrawMoveableTilemap(bool showTile = true);
        

        // entity.highlightedTilemap.SetTile(currentCellgridPosition, moveRangeHighlightedTileBase);

        /*for (int x = -moveRangeInHexGrid.x; x <= moveRangeInHexGrid.x; x++)
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
        }*/

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

    public virtual void ChangePieceType(PieceType pieceType)
    {
        this.pieceType = pieceType;
        Manager.Instance.gameManager.continueTurn = false;
        Manager.Instance.gameManager.TurnEnd();
    }

    protected bool CheckMovementCondition(PieceType pieceType, Vector3Int cellgridPosition)
    {
        if (pieceType == PieceType.Pawn)
        {
            return !pathfinder.objectTilemap.HasTile(cellgridPosition);
        }

        if (pieceType == PieceType.Knight)
        {
            Vector3Int movement = cellgridPosition - currentCellgridPosition;
            return !pathfinder.objectTilemap.HasTile(cellgridPosition) && !pathfinder.objectTilemap.HasTile(currentCellgridPosition + new Vector3Int(movement.x / 2, movement.y / 2, 0));
        }

        if (pieceType == PieceType.Bishop)
        {
            return !pathfinder.objectTilemap.HasTile(cellgridPosition);
        }

        if (pieceType == PieceType.Rook)
        {
            return !pathfinder.objectTilemap.HasTile(cellgridPosition) || pathfinder.objectTilemap.GetTile(cellgridPosition) == swampTileBase;
        }

        if (pieceType == PieceType.Queen)
        {
            return !pathfinder.objectTilemap.HasTile(cellgridPosition);
        }

        return false;
    }
}
