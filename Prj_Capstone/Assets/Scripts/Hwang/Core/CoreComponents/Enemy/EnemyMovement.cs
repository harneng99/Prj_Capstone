using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class EnemyMovement : Movement
{
    protected override void Start()
    {
        base.Start();

        smoothMoveFinished += AttackPiece;
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase && Manager.Instance.gameManager.playerPhase)
            {
                if (Manager.Instance.gameManager.prevSelectedEntity != null && Manager.Instance.gameManager.prevSelectedEntity.GetType().Equals(typeof(Player)) && InAttackRange((Player)Manager.Instance.gameManager.prevSelectedEntity))
                {
                    DrawMoveableTilemap(false);
                }
            }
        }
    }

    public bool CheckAttackArea()
    {
        bool foundTarget = false;

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        if (pieceType == PieceType.Pawn)
        {
            for (int x = -1; x <= 1; x += 2)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, -1, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        break;
                    }
                }
            }
        }

        if (pieceType == PieceType.Knight)
        {
            foreach (Vector3Int movement in knightMovements)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + movement;

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        break;
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

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        goto Next;
                    }
                }
            }

            for (int x = 1; x <= bounds.size.x; x++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        goto Next;
                    }
                }
            }

            for (int y = -1; y >= -bounds.size.x; y--)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        goto Next;
                    }
                }
            }

            for (int y = 1; y <= bounds.size.x; y++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                    if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                    {
                        foundTarget = true;
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        goto Next;
                    }
                }
            }
        }

    Next:
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

                        entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(Player));

                        if (entity.entityCombat.targetEntity != null && !entity.entityCombat.targetEntity.isDead)
                        {
                            foundTarget = true;
                            MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                            goto End;
                        }
                    }
                }
            }
        }

    End:
        if (!foundTarget)
        {
            Manager.Instance.gameManager.iterateNextEnemy = true;
        }
        else
        {
            Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(entity.transform);
        }

        return foundTarget;
    }

    public override void DrawMoveableTilemap(bool showTile = true)
    {
        // TODO: Should we draw enemy moveable tile when it is clicked?
        base.DrawMoveableTilemap(showTile);

        if (pieceType == PieceType.Pawn)
        {
            entity.highlightedTilemap.SetTile(currentCellgridPosition + Vector3Int.down, null);
        }
    }

    public bool InAttackRange(Player playerCharacter)
    {
        if (playerCharacter.isDead || entity.isDead)
        {
            return false;
        }

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        if (playerCharacter.playerMovement.pieceType == PieceType.Pawn)
        {
            for (int x = -1; x <= 1; x += 2)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + new Vector3Int(x, 1, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }
        }

        if (playerCharacter.playerMovement.pieceType == PieceType.Knight)
        {
            foreach (Vector3Int movement in knightMovements)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + movement;

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) continue;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }
        }

        if (playerCharacter.playerMovement.pieceType == PieceType.Rook || playerCharacter.playerMovement.pieceType == PieceType.Queen)
        {
            for (int x = -1; x >= -bounds.size.x; x--)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }
            for (int x = 1; x <= bounds.size.x; x++)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }

            for (int y = -1; y >= -bounds.size.x; y--)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }
            for (int y = 1; y <= bounds.size.x; y++)
            {
                Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                    if (moveableCellgridPosition == currentCellgridPosition)
                    {
                        return true;
                    }
                }
            }
        }

        if (playerCharacter.playerMovement.pieceType == PieceType.Bishop || playerCharacter.playerMovement.pieceType == PieceType.Queen)
        {
            int length = Mathf.Min(bounds.size.x, bounds.size.y);

            foreach (Vector3Int direction in bishopDirections)
            {
                for (int i = 1; i < length; i++)
                {
                    Vector3Int moveableCellgridPosition = playerCharacter.playerMovement.currentCellgridPosition + direction * i;

                    if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                    {
                        if (!CheckMovementCondition(pieceType, moveableCellgridPosition)) break;

                        if (moveableCellgridPosition == currentCellgridPosition)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void AttackPiece()
    {
        int availablePiece = 0;

        foreach (Player playerPiece in Manager.Instance.gameManager.playerPieces)
        {
            if (!playerPiece.isDead)
            {
                availablePiece += 1;
            }
        }

        if (Manager.Instance.gameManager.enemyPhase && availablePiece < Manager.Instance.gameManager.howManyShouldBeInTheGoal && !Manager.Instance.gameManager.gameFinished)
        {
            Manager.Instance.uiManager.ShowGameResultWindow("Stage Failed...");
            Manager.Instance.gameManager.gameFinished = true;
        }

        Manager.Instance.gameManager.iterateNextEnemy = true;
    }

    protected override bool AttackBeforeMove(PieceType pieceType)
    {
        if (pieceType == PieceType.Bishop || pieceType == PieceType.Queen)
        {
            return true;
        }
        else return false;
    }

    protected override bool AttackAfterMove(PieceType pieceType)
    {
        if (pieceType != PieceType.Bishop && pieceType != PieceType.Queen)
        {
            return true;
        }
        else return false;
    }

    public override void ResetEntityBooleanVariables()
    {
        didCurrentEntityMovedThisTurn = false;
    }
}
