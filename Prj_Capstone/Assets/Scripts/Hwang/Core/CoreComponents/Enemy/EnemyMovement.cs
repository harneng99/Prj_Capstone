using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : Movement
{
    public void CheckAttackArea()
    {
        Manager.Instance.gameManager.Select(entity);

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        if (pieceType == PieceType.Pawn)
        {
            for (int x = -1; x <= 1; x += 2)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, -1, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
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
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
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
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
                        goto Next;
                    }
                }
            }

            for (int x = 1; x <= bounds.size.x; x++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
                        goto Next;
                    }
                }
            }

            for (int y = -1; y >= -bounds.size.x; y--)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
                        goto Next;
                    }
                }
            }

            for (int y = 1; y <= bounds.size.x; y++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(0, y, 0);

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                    if (entity != null && entity.gameObject.activeSelf)
                    {
                        MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                        Action attackPiece = null;
                        attackPiece = () =>
                        {
                            entity.gameObject.SetActive(false);
                            Manager.Instance.gameManager.iterateNextEnemy = true;
                            smoothMoveFinished -= attackPiece;
                        };
                        smoothMoveFinished += attackPiece;
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
                        Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition, true, typeof(PlayerCharacter));

                        if (entity != null && entity.gameObject.activeSelf)
                        {
                            MoveToGrid(moveableCellgridPosition, GridType.Cellgrid, false);
                            Action attackPiece = null;
                            attackPiece = () =>
                            {
                                entity.gameObject.SetActive(false);
                                Manager.Instance.gameManager.iterateNextEnemy = true;
                                smoothMoveFinished -= attackPiece;
                            };
                            smoothMoveFinished += attackPiece;
                            goto End;
                        }
                    }
                }
            }
        }

    End:
        Manager.Instance.gameManager.iterateNextEnemy = true;
    }
}
