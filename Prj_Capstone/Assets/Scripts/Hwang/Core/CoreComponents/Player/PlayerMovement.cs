using Dev.ComradeVanti.WaitForAnim;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerMovement : Movement
{
    [SerializeField] private TileBase queenAbilityTileBase;
    [SerializeField] private TileBase pillarUpperPartTileBase;

    public bool invokePieceAbility { get; set; }

    private Player player;
    private int knightConsecutiveMovesCount;
    private bool dontInteractWithTile;
    private Vector3Int queenAbilityCellgridPosition;

    protected override void Awake()
    {
        base.Awake();

        player = entity as Player;
    }

    protected override void Start()
    {
        base.Start();

        smoothMoveFinished += Manager.Instance.gameManager.TurnEnd;
        smoothMoveFinished += () => { Manager.Instance.gameManager.Select(entity); };
        smoothMoveFinished += () => { Manager.Instance.uiManager.endTurnButton.interactable = false; };
        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
    }

    public override void DrawMoveableTilemap(bool showTile = true)
    {
        if (pieceType == PieceType.Pawn)
        {
            base.DrawMoveableTilemap(showTile);

            TileBase interactableTile = entity.interactableTilemap.GetTile(currentCellgridPosition);
            GameObject interactableTileGameObject = entity.interactableTilemap.GetInstantiatedObject(currentCellgridPosition);
            CustomTileData customTileData = interactableTileGameObject?.GetComponent<CustomTileData>();

            if (interactableTile != null)
            {
                if (customTileData?.interactableTileLayer == InteractableTileLayer.Promotion)
                {
                    if (pieceType == PieceType.Pawn)
                    {
                        player.canvas.gameObject.SetActive(true);
                    }
                }
            }
        }
        else if (pieceType == PieceType.Queen)
        {
            if (invokePieceAbility)
            {
                DrawQueenAbilityArea(showTile, currentCellgridPosition);
            }
            else
            {
                base.DrawMoveableTilemap(showTile);
            }
        }
        else
        {
            base.DrawMoveableTilemap(showTile);
        }
    }

    // This is called whenever the mouse clicks anywhere. It is different from OnPointerClick event function.
    public void MouseLeftClick()
    {
        // bool invokePieceAbility = false;

        if (!Manager.Instance.playerInputManager.IsPointerOverUI())
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                if (!isMoving && entity.isSelected)
                {
                    Vector3Int destinationCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                    TileBase highlightedTile = entity.highlightedTilemap.GetTile(destinationCellgridPosition);

                    // TODO: IsPointerOverUI does not work as expected.
                    if (Manager.Instance.gameManager.didEntityMovedThisTurn && highlightedTile != null)
                    {
                        if ((pieceType == PieceType.Knight || pieceType == PieceType.Queen) && invokePieceAbility)
                        {
                            Manager.Instance.gameManager.continueTurn = false;
                            invokePieceAbility = false;
                            goto MovementLogic;
                        }

                        Manager.Instance.uiManager.ShowWarningUI("Warning: Already Moved.");
                        return;
                    }

                 MovementLogic:
                    if (highlightedTile != null && destinationCellgridPosition != currentCellgridPosition)
                    {
                        #region Movement Logic
                        entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition, true, typeof(Enemy));
                        Manager.Instance.uiManager.endTurnButton.interactable = false;
                        if (player.canvas != null)
                        {
                            player.canvas.gameObject.SetActive(false);
                        }

                        if (highlightedTile.Equals(moveRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                if (pieceType == PieceType.Queen)
                                {
                                    invokePieceAbility = DrawQueenAbilityArea(false, destinationCellgridPosition);
                                }
                            }
                        }
                        else if (highlightedTile.Equals(attackRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                entity.entityCombat.targetEntity.isDead = true;

                                if (pieceType == PieceType.Knight && knightConsecutiveMovesCount == 0)
                                {
                                    invokePieceAbility = true;
                                }
                                else if (pieceType == PieceType.Bishop)
                                {
                                    invokePieceAbility = false;

                                    for (int x = -1; x <= 1; x++)
                                    {
                                        for (int y = -1; y <= 1; y++)
                                        {
                                            if (x == 0 && y == 0) continue;

                                            Entity entity = Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition + new Vector3Int(x, y, 0), true, typeof(Enemy));
                                            TileBase objectTileBase = pathfinder.objectTilemap.GetTile(destinationCellgridPosition + new Vector3Int(x, y, 0));

                                            if (entity != null || objectTileBase != null)
                                            {
                                                invokePieceAbility = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (pieceType == PieceType.Queen)
                                {
                                    invokePieceAbility = DrawQueenAbilityArea(false, destinationCellgridPosition);
                                }
                            }
                        }
                        else if (highlightedTile.Equals(queenAbilityTileBase))
                        {
                            dontInteractWithTile = true;
                            DrawMoveableTilemap(false);
                            queenAbilityCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);
                            Entity targetEntity = Manager.Instance.gameManager.EntityExistsAt(queenAbilityCellgridPosition);
                            if (targetEntity != null)
                            {
                                targetEntity.isDead = true;
                            }
                            entity.Flip(queenAbilityCellgridPosition.x - currentCellgridPosition.x);
                            entity.animator.SetTrigger("Ability");
                        }

                        if (pieceType == PieceType.Knight && knightConsecutiveMovesCount != 0)
                        {
                            invokePieceAbility = false;
                            knightConsecutiveMovesCount = 0;
                        }
                        #endregion

                        #region Interactable Tile Logic
                        TileBase interactableTile = entity.interactableTilemap.GetTile(destinationCellgridPosition);
                        GameObject interactableTileGameObject = entity.interactableTilemap.GetInstantiatedObject(destinationCellgridPosition);
                        CustomTileData customTileData = interactableTileGameObject?.GetComponent<CustomTileData>();

                        if (interactableTile != null && !dontInteractWithTile)
                        {
                            if (customTileData.interactableTileLayer == InteractableTileLayer.Promotion)
                            {
                                if (pieceType == PieceType.Pawn)
                                {
                                    invokePieceAbility = true;
                                }
                            }
                            else if (customTileData.interactableTileLayer == InteractableTileLayer.UnidirectionalTeleport && customTileData.entrance)
                            {
                                List<TeleportLabel> teleportLabels = FindObjectsOfType<GameObject>().Where(obj => obj.name.Contains("Teleport Label")).Select(obj => obj.GetComponent<TeleportLabel>()).ToList();
                                Label teleportEntranceLabel = teleportLabels.FirstOrDefault(teleportLabel => teleportLabel.cellgridPosition == destinationCellgridPosition).teleportLabel;

                                foreach (TeleportLabel teleportLabel in teleportLabels)
                                {
                                    CustomTileData currentCustomTileData = entity.interactableTilemap.GetInstantiatedObject(teleportLabel.cellgridPosition).GetComponent<CustomTileData>();

                                    if (currentCustomTileData.interactableTileLayer == InteractableTileLayer.UnidirectionalTeleport && !currentCustomTileData.entrance && teleportLabel.teleportLabel == teleportEntranceLabel && !Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition, true) && !(pieceType == PieceType.Queen && invokePieceAbility))
                                    {
                                        Action teleport = null;
                                        teleport = () =>
                                        {
                                            Debug.Log("Unidirectional Teleport function called");
                                            MoveToGrid(teleportLabel.cellgridPosition, GridType.Cellgrid, true);
                                            smoothMoveFinished -= teleport;
                                        };
                                        smoothMoveFinished += teleport;

                                        if (pieceType == PieceType.Queen)
                                        {
                                            invokePieceAbility = DrawQueenAbilityArea(false, teleportLabel.cellgridPosition);
                                        }
                                        else if (pieceType == PieceType.Bishop)
                                        {
                                            invokePieceAbility = false;

                                            for (int x = -1; x <= 1; x++)
                                            {
                                                for (int y = -1; y <= 1; y++)
                                                {
                                                    if (x == 0 && y == 0) continue;

                                                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition + new Vector3Int(x, y, 0), true, typeof(Enemy));
                                                    TileBase objectTileBase = pathfinder.objectTilemap.GetTile(teleportLabel.cellgridPosition + new Vector3Int(x, y, 0));

                                                    if (entity != null || objectTileBase != null)
                                                    {
                                                        invokePieceAbility = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            else if (customTileData.interactableTileLayer == InteractableTileLayer.BidirectionalTeleport)
                            {
                                List<TeleportLabel> teleportLabels = FindObjectsOfType<GameObject>().Where(obj => obj.name.Contains("Teleport Label")).Select(obj => obj.GetComponent<TeleportLabel>()).ToList();
                                Label teleportEntranceLabel = teleportLabels.FirstOrDefault(teleportLabel => teleportLabel.cellgridPosition == destinationCellgridPosition).teleportLabel;

                                foreach (TeleportLabel teleportLabel in teleportLabels)
                                {
                                    CustomTileData currentCustomTileData = entity.interactableTilemap.GetInstantiatedObject(teleportLabel.cellgridPosition).GetComponent<CustomTileData>();

                                    if (currentCustomTileData.interactableTileLayer == InteractableTileLayer.BidirectionalTeleport && teleportLabel.teleportLabel == teleportEntranceLabel && customTileData != currentCustomTileData && !Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition, true))
                                    {
                                        Action teleport = null;
                                        teleport = () =>
                                        {
                                            Debug.Log("Bidirectional Teleport function called");
                                            MoveToGrid(teleportLabel.cellgridPosition, GridType.Cellgrid, true);
                                            smoothMoveFinished -= teleport;
                                        };
                                        smoothMoveFinished += teleport;

                                        if (pieceType == PieceType.Queen)
                                        {
                                            invokePieceAbility = DrawQueenAbilityArea(false, teleportLabel.cellgridPosition);
                                        }
                                        else if (pieceType == PieceType.Bishop)
                                        {
                                            invokePieceAbility = false;

                                            for (int x = -1; x <= 1; x++)
                                            {
                                                for (int y = -1; y <= 1; y++)
                                                {
                                                    if (x == 0 && y == 0) continue;

                                                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition + new Vector3Int(x, y, 0), true, typeof(Enemy));
                                                    TileBase objectTileBase = pathfinder.objectTilemap.GetTile(teleportLabel.cellgridPosition + new Vector3Int(x, y, 0));

                                                    if (entity != null || objectTileBase != null)
                                                    {
                                                        invokePieceAbility = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            else if (customTileData.interactableTileLayer == InteractableTileLayer.Goal)
                            {
                                invokePieceAbility = false;

                                Action pieceArrived = null;
                                pieceArrived = () =>
                                {
                                    entity.gameObject.SetActive(false);
                                    Manager.Instance.gameManager.howManyCurrentInGoal += 1;
                                    Manager.Instance.uiManager.SetGoalCounter(true, "Goal " + Manager.Instance.gameManager.howManyCurrentInGoal + " / " + Manager.Instance.gameManager.howManyShouldBeInTheGoal);

                                    if (Manager.Instance.gameManager.howManyCurrentInGoal >= Manager.Instance.gameManager.howManyShouldBeInTheGoal)
                                    {
                                        PlayerPrefs.SetInt("stageclear", 1);
                                        Manager.Instance.gameManager.PauseGame();
                                        Manager.Instance.uiManager.ShowGameResultWindow("Game Clear!");
                                    }
                                };
                                smoothMoveFinished += pieceArrived;
                            }
                        }
                        #endregion
                    }
                    
                    #region Piece Ability
                    if (invokePieceAbility)
                    {
                        // TODO: For some unknown reason, function won't get unsubscribed immediately from the action.
                        Action pieceAbility = null;
                        Manager.Instance.gameManager.continueTurn = true;

                        if (pieceType == PieceType.Pawn)
                        {
                            pieceAbility = () =>
                            {
                                Debug.Log("Pawn Function Called");
                                if (pieceType == PieceType.Pawn)
                                {
                                    player.canvas.gameObject.SetActive(true);
                                }
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Knight)
                        {
                            pieceAbility = () =>
                            {
                                Debug.Log("Knight Function Called");
                                knightConsecutiveMovesCount += 1;
                                Manager.Instance.uiManager.endTurnButton.interactable = true;
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Bishop)
                        {
                            pieceAbility = () =>
                            {
                                Debug.Log("Bishop Function Called");
                                entity.animator.SetTrigger("Ability");
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Queen)
                        {
                            pieceAbility = () =>
                            {
                                Debug.Log("Queen Function Called");
                                DrawQueenAbilityArea(true, currentCellgridPosition);
                                smoothMoveFinished -= pieceAbility;
                            };
                        }

                        smoothMoveFinished += pieceAbility;
                    }
                    #endregion
                }
            }
        }
    }

    public override void ChangePieceType(PieceType pieceType)
    {
        base.ChangePieceType(pieceType);

        player.canvas.gameObject.SetActive(false);
    }

    private bool DrawQueenAbilityArea(bool showTile, Vector3Int centerCellgridPosition)
    {
        entity.highlightedTilemap.ClearAllTiles();

        bool invokeQueenAbility = false;

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        for (int x = -1; x >= -bounds.size.x; x--)
        {
            Vector3Int moveableCellgridPosition = centerCellgridPosition + new Vector3Int(x, 0, 0);

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                if (!CheckQueenAbilityCondition(moveableCellgridPosition)) break;

                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true);
                CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(moveableCellgridPosition)?.GetComponent<CustomTileData>();

                if (entity != null && !entity.isDead)
                {
                    if (entity.GetType().Equals(this.entity.GetType()))
                    {
                        break;
                    }
                    else
                    {
                        if (showTile)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        }
                        invokeQueenAbility = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    if (showTile)
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    }
                    invokeQueenAbility = true;
                    break;
                }
            }
        }
        for (int x = 1; x <= bounds.size.x; x++)
        {
            Vector3Int moveableCellgridPosition = centerCellgridPosition + new Vector3Int(x, 0, 0);

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                if (!CheckQueenAbilityCondition(moveableCellgridPosition)) break;

                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true);
                CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(moveableCellgridPosition)?.GetComponent<CustomTileData>();

                if (entity != null && !entity.isDead)
                {
                    if (entity.GetType().Equals(this.entity.GetType()))
                    {
                        break;
                    }
                    else
                    {
                        if (showTile)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        }
                        invokeQueenAbility = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    if (showTile)
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    }
                    invokeQueenAbility = true;
                    break;
                }
            }
        }

        for (int y = -1; y >= -bounds.size.x; y--)
        {
            Vector3Int moveableCellgridPosition = centerCellgridPosition + new Vector3Int(0, y, 0);

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                if (!CheckQueenAbilityCondition(moveableCellgridPosition)) break;

                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true);
                CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(moveableCellgridPosition)?.GetComponent<CustomTileData>();

                if (entity != null && !entity.isDead)
                {
                    if (entity.GetType().Equals(this.entity.GetType()))
                    {
                        break;
                    }
                    else
                    {
                        if (showTile)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        }
                        invokeQueenAbility = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    if (showTile)
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    }
                    invokeQueenAbility = true;
                    break;
                }
            }
        }
        for (int y = 1; y <= bounds.size.x; y++)
        {
            Vector3Int moveableCellgridPosition = centerCellgridPosition + new Vector3Int(0, y, 0);

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                if (!CheckQueenAbilityCondition(moveableCellgridPosition)) break;

                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true);
                CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(moveableCellgridPosition)?.GetComponent<CustomTileData>();

                if (entity != null && !entity.isDead)
                {
                    if (entity.GetType().Equals(this.entity.GetType()))
                    {
                        break;
                    }
                    else
                    {
                        if (showTile)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        }
                        invokeQueenAbility = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    if (showTile)
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    }
                    invokeQueenAbility = true;
                    break;
                }
            }
        }

        int length = Mathf.Min(bounds.size.x, bounds.size.y);

        foreach (Vector3Int direction in bishopDirections)
        {
            for (int i = 1; i < length; i++)
            {
                Vector3Int moveableCellgridPosition = centerCellgridPosition + direction * i;

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckQueenAbilityCondition(moveableCellgridPosition)) break;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true);
                    CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(moveableCellgridPosition)?.GetComponent<CustomTileData>();

                    if (entity != null && !entity.isDead)
                    {
                        if (entity.GetType().Equals(this.entity.GetType()))
                        {
                            break;
                        }
                        else
                        {
                            if (showTile)
                            {
                                this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                            }
                            invokeQueenAbility = true;
                            break;
                        }
                    }
                    else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                    {
                        if (showTile)
                        {
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        }
                        invokeQueenAbility = true;
                        break;
                    }
                }
            }
        }

        return invokeQueenAbility;
    }

    private bool CheckQueenAbilityCondition(Vector3Int cellgridPosition)
    {
        CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(cellgridPosition)?.GetComponent<CustomTileData>();
        return !pathfinder.objectTilemap.HasTile(cellgridPosition) || customTileData?.objectTileLayer == ObjectTileLayer.Wall;
    }

    protected override bool AttackBeforeMove(PieceType pieceType)
    {
        if (pieceType == PieceType.Bishop)
        {
            return true;
        }
        else return false;
    }

    protected override bool AttackAfterMove(PieceType pieceType)
    {
        if (pieceType != PieceType.Bishop)
        {
            return true;
        }
        else return false;
    }

    public override void ResetEntityBooleanVariables()
    {
        knightConsecutiveMovesCount = 0;
        didCurrentEntityMovedThisTurn = false;
        invokePieceAbility = false;
        dontInteractWithTile = false;
    }

    public void PieceAbility()
    {
        float minimumTime = 0.0f;
        Entity waitForEntity = null;
        CustomTileData waitForTileData = null;

        if (pieceType == PieceType.Bishop)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(currentCellgridPosition + new Vector3Int(x, y, 0), true, typeof(Enemy));
                    CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(currentCellgridPosition + new Vector3Int(x, y, 0))?.GetComponent<CustomTileData>();

                    if (entity != null || customTileData != null)
                    {
                        GameObject playerBishopAttackEffectGameObject = Manager.Instance.objectPoolingManager.GetGameObject("PlayerBishopAttackEffect0");

                        if (playerBishopAttackEffectGameObject != null)
                        {
                            Animator animator = playerBishopAttackEffectGameObject.GetComponent<Animator>();
                            minimumTime = animator.GetCurrentAnimatorStateInfo(0).length;
                            AttackEffect attackEffect = playerBishopAttackEffectGameObject.GetComponent<AttackEffect>();
                            attackEffect.SetAttackEffectTarget(this.entity, currentCellgridPosition + new Vector3Int(x, y, 0));
                        }

                        if (entity != null) waitForEntity = entity;
                        if (customTileData != null) waitForTileData = customTileData;
                    }
                }
            }
        }
        else if (pieceType == PieceType.Queen)
        {
            Entity entity = Manager.Instance.gameManager.EntityExistsAt(queenAbilityCellgridPosition, false, typeof(Enemy));
            CustomTileData customTileData = pathfinder.objectTilemap.GetInstantiatedObject(queenAbilityCellgridPosition)?.GetComponent<CustomTileData>();

            if (entity != null || customTileData != null)
            {
                GameObject playerQueenAttackEffectGameObject = Manager.Instance.objectPoolingManager.GetGameObject("PlayerQueenAttackEffect0");

                if (playerQueenAttackEffectGameObject != null)
                {
                    Animator animator = playerQueenAttackEffectGameObject.GetComponent<Animator>();
                    minimumTime = animator.GetCurrentAnimatorStateInfo(0).length;
                    AttackEffect attackEffect = playerQueenAttackEffectGameObject.GetComponent<AttackEffect>();
                    attackEffect.SetAttackEffectTarget(this.entity, queenAbilityCellgridPosition);
                }

                if (entity != null) waitForEntity = entity;
                if (customTileData != null) waitForTileData = customTileData;
            }
        }

        StartCoroutine(CallTurnEndFunction(waitForEntity, waitForTileData, minimumTime));
    }

    private IEnumerator CallTurnEndFunction(Entity entity, CustomTileData customTileData, float minimumTime)
    {
        yield return new WaitForSeconds(minimumTime);

        if (entity != null)
        {
            yield return new WaitForAnimationToStart(entity.animator, "Death");
            yield return new WaitForAnimationToFinish(entity.animator, "Death");
        }
        else if (customTileData != null)
        {
            yield return new WaitForAnimationToStart(customTileData.animator, "Destroy");
            yield return new WaitForAnimationToFinish(customTileData.animator, "Destroy");
        }

        Manager.Instance.gameManager.continueTurn = false;
        Manager.Instance.gameManager.TurnEnd();
    }
}
