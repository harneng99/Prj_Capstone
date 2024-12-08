using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Dev.ComradeVanti.WaitForAnim;

public enum GridType { Hexgrid, Cellgrid }
public enum PieceType { Pawn, Knight, Bishop, Rook, Queen }

[RequireComponent(typeof(Pathfinder))]
public abstract class Movement : CoreComponent
{
    [field: SerializeField] public Vector3Int moveRangeInHexGrid { get; protected set; }
    [field: SerializeField] public PieceType pieceType { get; protected set; }
    [SerializeField] protected TileBase moveRangeHighlightedTileBase;
    [SerializeField] protected TileBase attackRangeHighlightedTileBase;
    [SerializeField] protected int maxMovementStamina;

    public Pathfinder pathfinder { get; private set; }
    public Vector3 currentWorldgridPosition { get; set; }
    public Vector3Int currentCellgridPosition { get; private set; }
    public Vector3Int currentHexgridPosition { get; private set; }

    public event Action smoothMoveFinished;

    public bool isMoving { get; protected set; }
    protected bool playWalkSound;
    protected bool didCurrentEntityMovedThisTurn;
    protected Coroutine smoothMovementCoroutine;
    protected AudioSource audioSource;

    protected Vector3Int[] knightMovements = { new Vector3Int(-1, 2, 0), new Vector3Int(1, 2, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, -1, 0), new Vector3Int(1, -2, 0), new Vector3Int(-1, -2, 0), new Vector3Int(-2, 1, 0), new Vector3Int(-2, -1, 0) };
    protected Vector3Int[] bishopDirections = { new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) };

    protected override void Awake()
    {
        base.Awake();

        pathfinder = GetComponent<Pathfinder>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        currentCellgridPosition = pathfinder.moveableTilemap.WorldToCell(entity.GetEntityFeetPosition());
        currentHexgridPosition = pathfinder.CellgridToHexgrid(currentCellgridPosition);
        // entity.transform.position = pathfinder.moveableTilemap.CellToWorld(currentCellgridPosition);
        entity.SetEntityPosition(currentCellgridPosition);
        UpdateGridPositionData();

        smoothMoveFinished += () =>
        {
            isMoving = false;
            playWalkSound = false;
            entity.entityCombat.targetEntity = null;
            entity.animator.SetBool("Move", false);
        };
    }

    private void Update()
    {
        if (playWalkSound)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase && !isMoving)
            {
                if (Manager.Instance.gameManager.currentSelectedEntity.Equals(entity) && Manager.Instance.gameManager.currentSelectedEntity.Equals(Manager.Instance.gameManager.prevSelectedEntity))
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

        if (currentCellgridPosition.Equals(destinationCellgridPosition)) return false;

        didCurrentEntityMovedThisTurn = true;
        Manager.Instance.gameManager.didEntityMovedThisTurn = true;
        DrawMoveableTilemap(false);

        if (instantMove)
        {
            GameObject tileGameObject = pathfinder.moveableTilemap.GetInstantiatedObject(destinationCellgridPosition);

            if (tileGameObject != null)
            {
                if (!pathfinder.IsObstacle(destinationCellgridPosition) && pathfinder.IsMoveable(destinationCellgridPosition))
                {
                    // TODO: When there is target entity
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

            isMoving = true;

            if (smoothMovementCoroutine != null)
            {
                StopCoroutine(smoothMovementCoroutine);
            }
            smoothMovementCoroutine = StartCoroutine(MoveEntitySmooth(pathInformation.path));
            return true;
        }
    }

    protected virtual IEnumerator MoveToAttackPosition(Vector3 worldgridCellPosition)
    {
        if (entity.entityCombat.targetEntity == null)
        {
            Debug.LogWarning($"{entity}'s target entity value is null.");
            yield break;
        }

        Vector3 direction = entity.facingDirection * Vector3.right;
        entity.entityCombat.targetEntity.Flip(-direction.x);
        entity.entityCombat.targetEntity.animator.SetBool("Move", true);

        Vector3 entityDestinationWorldgridPosition = worldgridCellPosition - pathfinder.gridBase.cellSize.x * 0.5f * direction;
        Vector3 targetDestinationWorldgridPosition = worldgridCellPosition + pathfinder.gridBase.cellSize.x * 0.5f * direction;

        while (Vector3.Distance(entity.transform.position, entityDestinationWorldgridPosition) > epsilon || Vector3.Distance(entity.entityCombat.targetEntity.transform.position, targetDestinationWorldgridPosition) > epsilon)
        {
            entity.transform.position = Vector3.MoveTowards(entity.transform.position, entityDestinationWorldgridPosition, entity.entityConsistentData.movementVelocity * Time.deltaTime);
            if (Vector3.Distance(entity.transform.position, entityDestinationWorldgridPosition) < epsilon)
            {
                entity.animator.SetBool("Move", false);
            }

            entity.entityCombat.targetEntity.transform.position = Vector3.MoveTowards(entity.entityCombat.targetEntity.transform.position, targetDestinationWorldgridPosition, entity.entityCombat.targetEntity.entityConsistentData.movementVelocity * Time.deltaTime);
            if (Vector3.Distance(entity.entityCombat.targetEntity.transform.position, targetDestinationWorldgridPosition) < epsilon)
            {
                entity.entityCombat.targetEntity.animator.SetBool("Move", false);
            }
            yield return null;
        }

        yield break;
    }

    private IEnumerator MovementCoroutine(List<GridNode> path, int nthFromTheBack = 0)
    {
        entity.animator.SetBool("Move", true);

        if (pieceType != PieceType.Knight)
        {
            playWalkSound = true;
            while (path.Count > nthFromTheBack)
            {
                Vector3 currentDestinationWorldgridPosition = path.First().worldgridPosition;

                if (Vector3.Distance(entity.transform.position, currentDestinationWorldgridPosition) < epsilon)
                {
                    UpdateGridPositionData();

                    path.RemoveAt(0);

                    if (path.Count > 0)
                    {
                        currentDestinationWorldgridPosition = path.First().worldgridPosition;
                    }
                    else
                    {
                        break;
                    }
                }

                entity.transform.position = Vector3.MoveTowards(entity.transform.position, currentDestinationWorldgridPosition, entity.entityConsistentData.movementVelocity * Time.deltaTime);

                yield return null;
            }
        }
        else
        {
            Manager.Instance.soundFXManager.PlaySoundFXClip(audioSource.clip, transform);
            entity.animator.SetBool("Jump", true);
            float duration = 0.5f; // entity.animator.GetCurrentAnimatorStateInfo(0).normalizedTime / 2.0f;
            Vector3 destinationWorldgridPosition = path.Last().worldgridPosition;
            if (entity.entityCombat.targetEntity != null)
            {
                destinationWorldgridPosition -= Vector3.right * entity.facingDirection;
            }

            yield return new WaitForSeconds(duration);

            entity.transform.position = destinationWorldgridPosition;
            entity.animator.SetBool("Jump", false);

            Manager.Instance.soundFXManager.PlaySoundFXClip(audioSource.clip, transform);
            if (nthFromTheBack == 0)
            {
                yield return new WaitForAnimationToFinish(entity.animator, "Jump");
            }
            playWalkSound = true;
        }
        
        yield break;
    }

    protected virtual IEnumerator MoveEntitySmooth(List<GridNode> path)
    {
        #region Initial Information Setting
        Vector3 initialWorldgridPosition = path.First().worldgridPosition;
        Vector3 destinationWorldgridPosition = path.Last().worldgridPosition;

        path.RemoveAt(0);
        if (path.Count <= 0)
        {
            Debug.LogWarning($"Wrong path calculation on entity {entity.name} from world grid {initialWorldgridPosition} to {destinationWorldgridPosition}.");
            yield break;
        }
        #endregion

        #region Sprite Setting
        entity.spriteRenderer.sortingOrder = 1;

        if (Mathf.Abs(destinationWorldgridPosition.x - initialWorldgridPosition.x) > epsilon)
        {
            entity.Flip(destinationWorldgridPosition.x - initialWorldgridPosition.x);
            Vector3 direction = entity.facingDirection * Vector3.right;
        }
        #endregion

        if (entity.entityCombat.targetEntity == null)
        {
            yield return MovementCoroutine(path);
        }
        else
        {
            if (AttackBeforeMove(pieceType))
            {
                entity.animator.SetInteger("AttackType", UnityEngine.Random.Range(0, entity.entityCombat.attackTypeCount));
                entity.animator.SetTrigger("Attack");
                
                yield return new WaitForAnimationToStart(entity.entityCombat.targetEntity.animator, "Death");
                yield return new WaitForAnimationToFinish(entity.entityCombat.targetEntity.animator, "Death");

                playWalkSound = true;
                yield return MovementCoroutine(path);
            }
            else if (AttackAfterMove(pieceType))
            {
                playWalkSound = pieceType != PieceType.Knight;

                yield return MovementCoroutine(path, 1);
                yield return MoveToAttackPosition(destinationWorldgridPosition);

                playWalkSound = false;
                entity.animator.SetInteger("AttackType", UnityEngine.Random.Range(0, entity.entityCombat.attackTypeCount));
                entity.animator.SetTrigger("Attack");
                string animationName = "Attack" + entity.animator.GetInteger("AttackType").ToString();
                yield return new WaitForAnimationToStart(entity.animator, animationName);
                yield return new WaitForAnimationToFinish(entity.animator, animationName);
            }

            if (entity.entityCombat.targetEntity.gameObject.activeSelf)
            {
                yield return new WaitForAnimationToFinish(entity.entityCombat.targetEntity.animator, "Hurt");
                yield return new WaitForAnimationToFinish(entity.entityCombat.targetEntity.animator, "Death");
            }
        }

        playWalkSound = true;
        entity.animator.SetBool("Move", true);
        while (Vector3.Distance(entity.transform.position, destinationWorldgridPosition) > epsilon)
        {
            entity.transform.position = Vector3.MoveTowards(entity.transform.position, destinationWorldgridPosition, entity.entityConsistentData.movementVelocity * Time.deltaTime);
            yield return null;
        }

        UpdateGridPositionData();
        smoothMoveFinished?.Invoke();
        entity.spriteRenderer.sortingOrder = 0;
    }

    protected abstract bool AttackBeforeMove(PieceType pieceType);

    protected abstract bool AttackAfterMove(PieceType pieceType);

    /// <summary>
    /// Gets whether entity is going to show moveable tile area in bool value. True means it will show its moveable tile area, and vice versa.
    /// </summary>
    /// <param name="showTile"></param>
    public virtual void DrawMoveableTilemap(bool showTile = true)
    {
        entity.highlightedTilemap.ClearAllTiles();

        if (!showTile || entity.isDead) return;

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        if (pieceType == PieceType.Pawn)
        {
            if (entity.GetType().Equals(typeof(Player)))
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 1, 0);

                    if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                    {
                        if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                        Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                        if (entity != null && !entity.isDead)
                        {
                            if (entity.GetType().Equals(typeof(Enemy)) && x != 0)
                            {
                                this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            }
                        }
                        else if (x == 0)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                        }
                    }
                }
            }
            else if (entity.GetType().Equals(typeof(Enemy)))
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, -1, 0);

                    if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                    {
                        if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                        Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                        if (entity != null && !entity.isDead)
                        {
                            if (entity.GetType().Equals(typeof(Player)) && x != 0)
                            {
                                this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            }
                        }
                        else if (x != 0)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                        }
                    }
                }
            }
        }

        if (pieceType == PieceType.Knight)
        {
            // entity.highlightedTilemap.SetTile(currentCellgridPosition, null);

            foreach (Vector3Int movement in knightMovements)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + movement;

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            continue;
                        }
                        else
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                        }
                    }
                    else
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                    }
                }
            }
        }

        if (pieceType == PieceType.Rook || pieceType == PieceType.Queen)
        {
            for (int x = -1; x >= -bounds.size.x; x--)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            break;
                        }
                        else
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            break;
                        }
                    }
                    else
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                    }
                }
            }
            for (int x = 1; x <= bounds.size.x; x++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            break;
                        }
                        else
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            break;
                        }
                    }
                    else
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                    }
                }
            }

            for (int y = -1; y >= -bounds.size.x; y--)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            break;
                        }
                        else
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            break;
                        }
                    }
                    else
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                    }
                }
            }
            for (int y = 1; y <= bounds.size.x; y++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            break;
                        }
                        else
                        {
                            entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                            break;
                        }
                    }
                    else
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
                    }
                }
            }
        }

        if (pieceType == PieceType.Bishop || pieceType == PieceType.Queen)
        {
            int length = Mathf.Min(bounds.size.x, bounds.size.y);

            foreach (Vector3Int direction in bishopDirections)
            {
                for (int i = 1; i < length; i++)
                {
                    Vector3Int moveableCellgridPosition = currentCellgridPosition + direction * i;

                    if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                    {
                        if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                        Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, onlyFindActive: true);

                        if (entity != null && !entity.isDead)
                        {
                            if (entity.GetType().Equals(this.entity.GetType()))
                            {
                                break;
                            }
                            else
                            {
                                entity.highlightedTilemap.SetTile(moveableCellgridPosition, attackRangeHighlightedTileBase);
                                break;
                            }
                        }
                        else
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, moveRangeHighlightedTileBase);
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

    public virtual void ChangePieceType(PieceType pieceType)
    {
        this.pieceType = pieceType;
        entity.animator.SetInteger("PieceType", (int)pieceType);
        if (pieceType == PieceType.Rook)
        {
            pathfinder.ChangeObjectLayerType(ObjectTileLayer.Swamp, false);
        }
        entity.highlightedTilemap.ClearAllTiles();

        if (Manager.Instance.gameManager.didEntityMovedThisTurn)
        {
            Manager.Instance.gameManager.continueTurn = false;
            Manager.Instance.gameManager.TurnEnd();
        }
    }

    protected virtual bool CheckMovementCondition(PieceType pieceType, Vector3Int cellgridPosition)
    {
        return pathfinder.IsMoveable(cellgridPosition) && !pathfinder.IsObstacle(cellgridPosition);
        /*if (pieceType == PieceType.Pawn)
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
            CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(cellgridPosition)?.GetComponent<CustomTileData>();
            return !pathfinder.objectTilemap.HasTile(cellgridPosition) || customTileData?.objectTileLayer == ObjectTileLayer.Swamp;
        }

        if (pieceType == PieceType.Queen)
        {
            return !pathfinder.objectTilemap.HasTile(cellgridPosition);
        }

        return false;*/
    }

    public abstract void ResetEntityBooleanVariables();
}
