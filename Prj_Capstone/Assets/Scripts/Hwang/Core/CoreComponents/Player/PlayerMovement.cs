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

    private PlayerCharacter playerCharacter;
    private Tilemap foregroundDecorationTilemap;
    private int knightConsecutiveMovesCount;

    protected override void Awake()
    {
        base.Awake();

        playerCharacter = entity as PlayerCharacter;
        foregroundDecorationTilemap = GameObject.FindWithTag("ForegroundDecorationTilemap").GetComponent<Tilemap>();
    }

    protected override void Start()
    {
        base.Start();

        smoothMoveFinished += () =>
        {
            if (pieceType == PieceType.Queen)
            {
                Manager.Instance.gameManager.continueTurn = DrawQueenAbilityArea(true);
            }
            Manager.Instance.gameManager.Select(entity);
        };
        // smoothMoveFinished += Manager.Instance.gameManager.TurnEnd;
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
                        playerCharacter.canvas.gameObject.SetActive(true);
                    }
                }
            }
        }
        else if (pieceType == PieceType.Queen)
        {
            if (invokePieceAbility)
            {
                DrawQueenAbilityArea(showTile);
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
        bool invokePieceAbility = false;

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
                            goto MovementLogic;
                        }

                        Manager.Instance.uiManager.ShowWarningUI("Warning: Already Moved.");
                        return;
                    }

                 MovementLogic:
                    if (highlightedTile != null && destinationCellgridPosition != currentCellgridPosition)
                    {
                        entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition, true, typeof(Enemy));
                        Manager.Instance.uiManager.endTurnButton.interactable = false;
                        playerCharacter.canvas.gameObject.SetActive(false);

                        if (highlightedTile.Equals(moveRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                if (pieceType == PieceType.Queen)
                                {
                                    invokePieceAbility = true;
                                }
                            }
                        }
                        else if (highlightedTile.Equals(attackRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                entity.entityCombat.targetEntity.isDead = true;

                                if (pieceType == PieceType.Bishop || pieceType == PieceType.Knight || pieceType == PieceType.Queen)
                                {
                                    invokePieceAbility = true;
                                }
                            }
                        }
                        else if (highlightedTile.Equals(queenAbilityTileBase))
                        {
                            DrawMoveableTilemap(false);
                            GameObject attackEffect = Manager.Instance.objectPoolingManager.GetGameObject(entity.GetType().Name + pieceType.ToString() + "AttackEffect");
                            attackEffect.transform.position = destinationCellgridPosition + new Vector3(0.5f, 0.5f, 0.0f);
                        }

                        if (pieceType == PieceType.Knight && knightConsecutiveMovesCount != 0)
                        {
                            invokePieceAbility = false;
                            knightConsecutiveMovesCount = 0;
                        }

                        #region Interactable Tile Logic
                        TileBase interactableTile = entity.interactableTilemap.GetTile(destinationCellgridPosition);
                        GameObject interactableTileGameObject = entity.interactableTilemap.GetInstantiatedObject(destinationCellgridPosition);
                        CustomTileData customTileData = interactableTileGameObject?.GetComponent<CustomTileData>();

                        if (interactableTile != null)
                        {
                            if (customTileData.interactableTileLayer == InteractableTileLayer.Promotion)
                            {
                                if (pieceType == PieceType.Pawn)
                                {
                                    invokePieceAbility = true;
                                    Manager.Instance.gameManager.continueTurn = true;
                                    
                                    Action openPromotion = null;
                                    openPromotion = () =>
                                    {
                                        playerCharacter.canvas.gameObject.SetActive(true);
                                        smoothMoveFinished -= openPromotion;
                                    };
                                    smoothMoveFinished += openPromotion;
                                }
                            }
                            else if (customTileData.interactableTileLayer == InteractableTileLayer.UnidirectionalTeleport && customTileData.entrance)
                            {
                                List<TeleportLabel> teleportLabels = FindObjectsOfType<GameObject>().Where(obj => obj.name.Contains("Teleport Label")).Select(obj => obj.GetComponent<TeleportLabel>()).ToList();
                                Label teleportEntranceLabel = teleportLabels.FirstOrDefault(teleportLabel => teleportLabel.cellgridPosition == destinationCellgridPosition).teleportLabel;

                                foreach (TeleportLabel teleportLabel in teleportLabels)
                                {
                                    CustomTileData currentCustomTileData = entity.interactableTilemap.GetInstantiatedObject(teleportLabel.cellgridPosition).GetComponent<CustomTileData>();

                                    if (currentCustomTileData.interactableTileLayer == InteractableTileLayer.UnidirectionalTeleport && !currentCustomTileData.entrance && teleportLabel.teleportLabel == teleportEntranceLabel && !Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition))
                                    {
                                        Action teleport = null;
                                        teleport = () =>
                                        {
                                            MoveToGrid(teleportLabel.cellgridPosition, GridType.Cellgrid, true);
                                            smoothMoveFinished -= teleport;
                                        };
                                        smoothMoveFinished += teleport;
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

                                    if (currentCustomTileData.interactableTileLayer == InteractableTileLayer.BidirectionalTeleport && teleportLabel.teleportLabel == teleportEntranceLabel && customTileData != currentCustomTileData && !Manager.Instance.gameManager.EntityExistsAt(teleportLabel.cellgridPosition))
                                    {
                                        Action teleport = null;
                                        teleport = () =>
                                        {
                                            MoveToGrid(teleportLabel.cellgridPosition, GridType.Cellgrid, true);
                                            smoothMoveFinished -= teleport;
                                        };
                                        smoothMoveFinished += teleport;
                                        break;
                                    }
                                }
                            }
                            else if (customTileData.interactableTileLayer == InteractableTileLayer.Goal)
                            {
                                invokePieceAbility = false;
                                Manager.Instance.gameManager.howManyCurrentInGoal += 1;
                                Manager.Instance.gameManager.gameFinished = Manager.Instance.gameManager.howManyCurrentInGoal >= Manager.Instance.gameManager.howManyShouldBeInTheGoal;

                                Action pieceArrived = null;
                                pieceArrived = () =>
                                {
                                    entity.gameObject.SetActive(false);

                                    if (Manager.Instance.gameManager.gameFinished)
                                    {
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
                        Action pieceAbility = null;
                        Manager.Instance.gameManager.continueTurn = true;

                        if (pieceType == PieceType.Pawn)
                        {
                            pieceAbility = () =>
                            {
                                playerCharacter.canvas.gameObject.SetActive(true);
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Knight)
                        {
                            pieceAbility = () =>
                            {
                                knightConsecutiveMovesCount += 1;
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Bishop)
                        {
                            pieceAbility = () =>
                            {
                                entity.animator.SetTrigger("Attack");

                                for (int x = -1; x <= 1; x++)
                                {
                                    for (int y  = -1; y <= 1; y++)
                                    {
                                        Vector3 bishopAttackSpawnPosition = currentWorldgridPosition.Value + new Vector3(x, y, 0);
                                        GameObject attackEffect = Manager.Instance.objectPoolingManager.GetGameObject(entity.GetType().Name + pieceType.ToString() + "AttackEffect");
                                        attackEffect.transform.position = bishopAttackSpawnPosition;
                                    }
                                }
                                smoothMoveFinished -= pieceAbility;
                            };
                        }
                        else if (pieceType == PieceType.Queen)
                        {
                            pieceAbility = () =>
                            {
                                Manager.Instance.gameManager.continueTurn = DrawQueenAbilityArea(true);
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

        playerCharacter.canvas.gameObject.SetActive(false);
    }

    private bool DrawQueenAbilityArea(bool showTile)
    {
        entity.highlightedTilemap.ClearAllTiles();
        
        if (!showTile) return false;

        bool actuallyDrawSomething = false;

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        for (int x = -1; x >= -bounds.size.x; x--)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

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
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        actuallyDrawSomething = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    actuallyDrawSomething = true;
                    break;
                }
            }
        }
        for (int x = 1; x <= bounds.size.x; x++)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

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
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        actuallyDrawSomething = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    actuallyDrawSomething = true;
                    break;
                }
            }
        }

        for (int y = -1; y >= -bounds.size.x; y--)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

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
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        actuallyDrawSomething = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    actuallyDrawSomething = true;
                    break;
                }
            }
        }
        for (int y = 1; y <= bounds.size.x; y++)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

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
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        actuallyDrawSomething = true;
                        break;
                    }
                }
                else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                {
                    this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                    actuallyDrawSomething = true;
                    break;
                }
            }
        }

        int length = Mathf.Min(bounds.size.x, bounds.size.y);

        foreach (Vector3Int direction in bishopDirections)
        {
            for (int i = 1; i < length; i++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + direction * i;

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
                            this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                            actuallyDrawSomething = true;
                            break;
                        }
                    }
                    else if (customTileData?.objectTileLayer == ObjectTileLayer.Wall)
                    {
                        this.entity.highlightedTilemap.SetTile(moveableCellgridPosition, queenAbilityTileBase);
                        actuallyDrawSomething = true;
                        break;
                    }
                }
            }
        }

        return actuallyDrawSomething;
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
        didCurrentEntityMovedThisTurn = false;
        invokePieceAbility = false;
    }

    public void PieceAbility()
    {
        if (pieceType == PieceType.Bishop)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int abilityApplyCellgridPosition = currentCellgridPosition + new Vector3Int(x, y, 0);
                    pathfinder.objectTilemap.SetTile(abilityApplyCellgridPosition, null);
                    if (foregroundDecorationTilemap.GetTile(abilityApplyCellgridPosition + Vector3Int.up) == pillarUpperPartTileBase)
                    {
                        foregroundDecorationTilemap.SetTile(abilityApplyCellgridPosition + Vector3Int.up, null);
                    }
                    foregroundDecorationTilemap.SetTile(currentCellgridPosition + new Vector3Int(x, y + 1, 0), null);
                    Destroy(pathfinder.objectTilemap.GetInstantiatedObject(abilityApplyCellgridPosition));
                    Manager.Instance.gameManager.EntityExistsAt(abilityApplyCellgridPosition, true, typeof(Enemy))?.gameObject.SetActive(false);
                }
            }
        }
        else if (pieceType == PieceType.Queen)
        {
            Vector3Int destinationCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

            entity.highlightedTilemap.ClearAllTiles();
            pathfinder.objectTilemap.SetTile(destinationCellgridPosition, null);
            if (foregroundDecorationTilemap.GetTile(destinationCellgridPosition + Vector3Int.up) == pillarUpperPartTileBase)
            {
                foregroundDecorationTilemap.SetTile(destinationCellgridPosition + Vector3Int.up, null);
            }
            Destroy(pathfinder.objectTilemap.GetInstantiatedObject(destinationCellgridPosition));
            Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition, true, typeof(Enemy))?.gameObject.SetActive(false);
            Manager.Instance.gameManager.continueTurn = false;
            Manager.Instance.gameManager.TurnEnd();
        }
    }
}
