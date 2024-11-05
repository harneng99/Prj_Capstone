using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerMovement : Movement
{
    [SerializeField] protected TileBase promotionTileBase;
    [SerializeField] protected TileBase unidirectionalTeleportTileBaseEntrance;
    [SerializeField] protected TileBase unidirectionalTeleportTileBaseExit;
    [SerializeField] protected TileBase bidirectionalTeleportTileBase;

    private PlayerCharacter playerCharacter;

    private int knightConsecutiveMovesCount;

    protected override void Awake()
    {
        base.Awake();

        playerCharacter = entity as PlayerCharacter;
    }

    protected override void Start()
    {
        base.Start();

        smoothMoveFinished += Manager.Instance.gameManager.TurnEnd;
        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
    }

    public override void DrawMoveableTilemap(bool showTile = true)
    {
        base.DrawMoveableTilemap(showTile);

        if (pieceType == PieceType.Pawn)
        {
            TileBase interactableTile = entity.interactableTilemap.GetTile(currentCellgridPosition);
            GameObject interactableTileGameObject = entity.interactableTilemap.GetInstantiatedObject(currentCellgridPosition);
            CustomTileData customTileData = interactableTileGameObject?.GetComponent<CustomTileData>();

            if (interactableTile != null)
            {
                if (customTileData.interactableTileLayer == InteractableTileLayer.Promotion)
                {
                    if (pieceType == PieceType.Pawn)
                    {
                        playerCharacter.canvas.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    // This is called whenever the mouse clicks anywhere. It is different from OnPointerClick event function.
    public void MouseLeftClick()
    {
        if (!Manager.Instance.playerInputManager.IsPointerOverUI())
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                if (!isMoving && entity.isSelected)
                {
                    Vector3Int destinationCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                    TileBase highlightedTile = entity.highlightedTilemap.GetTile(destinationCellgridPosition);

                    if (entity.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Paralyze) || entity.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Stun) || entity.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Horror))
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Selected mercenary can't move.");
                        return;
                    }

                    // TODO: IsPointerOverUI does not work as expected.
                    if (Manager.Instance.gameManager.alreadyMoved && highlightedTile != null)
                    {
                        if (pieceType == PieceType.Knight && Manager.Instance.gameManager.continueTurn)
                        {
                            goto Exception;
                        }
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Already Moved.");
                        return;
                    }

                 Exception:
                    if (highlightedTile != null && destinationCellgridPosition != currentCellgridPosition)
                    {
                        if (highlightedTile.Equals(moveRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                DrawMoveableTilemap(false);
                                Manager.Instance.gameManager.alreadyMoved = true;
                            }
                        }
                        else if (highlightedTile.Equals(attackRangeHighlightedTileBase))
                        {
                            if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                            {
                                DrawMoveableTilemap(false);

                                Action attackPiece = null;
                                attackPiece = () =>
                                {
                                    Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition, true, typeof(Enemy))?.gameObject.SetActive(false);

                                    if (pieceType == PieceType.Bishop)
                                    {
                                        // TODO: Play animation

                                        for (int x = -1; x <= 1; x++)
                                        {
                                            for (int y = -1; y <= 1; y++)
                                            {
                                                pathfinder.objectTilemap.SetTile(destinationCellgridPosition + new Vector3Int(x, y, 0), null);
                                                Manager.Instance.gameManager.EntityExistsAt(destinationCellgridPosition + new Vector3Int(x, y, 0), true, typeof(Enemy))?.gameObject.SetActive(false);
                                            }
                                        }
                                    }

                                    if (pieceType == PieceType.Queen)
                                    {
                                        // TODO: Apply queen ability
                                    }
                                    smoothMoveFinished -= attackPiece;
                                };
                                smoothMoveFinished += attackPiece;

                                if (pieceType == PieceType.Knight && knightConsecutiveMovesCount == 0)
                                {
                                    Manager.Instance.gameManager.continueTurn = true;
                                    knightConsecutiveMovesCount += 1;
                                    Manager.Instance.gameManager.alreadyMoved = true;
                                    goto InteractableTileLogic;
                                }

                                Manager.Instance.gameManager.alreadyMoved = true;
                            }
                        }

                        if (pieceType == PieceType.Knight && knightConsecutiveMovesCount != 0)
                        {
                            Manager.Instance.gameManager.continueTurn = false;
                            knightConsecutiveMovesCount = 0;
                        }

                    InteractableTileLogic:
                        TileBase interactableTile = entity.interactableTilemap.GetTile(destinationCellgridPosition);
                        GameObject interactableTileGameObject = entity.interactableTilemap.GetInstantiatedObject(destinationCellgridPosition);
                        CustomTileData customTileData = interactableTileGameObject?.GetComponent<CustomTileData>();

                        if (interactableTile != null)
                        {
                            if (customTileData.interactableTileLayer == InteractableTileLayer.Promotion)
                            {
                                if (pieceType == PieceType.Pawn)
                                {
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
                    }
                }
            }
        }
    }

    public override void ChangePieceType(PieceType pieceType)
    {
        base.ChangePieceType(pieceType);

        playerCharacter.canvas.gameObject.SetActive(false);
    }
}
