using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class QueenMovement : PlayerMovement
{
    private Vector3Int[] directions = { new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) };

    public override void DrawMoveableTilemap(bool showTile = true)
    {
        base.DrawMoveableTilemap(showTile);

        if (!showTile) return;

        entity.highlightedTilemap.SetTile(currentCellgridPosition, moveRangeHighlightedTileBase);

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

        for (int x = -1; x >= -bounds.size.x; x--)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + new Vector3Int(x, 0, 0);

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        break;
                    }
                    else if (entity.GetType().Equals(typeof(Enemy)))
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
                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        break;
                    }
                    else if (entity.GetType().Equals(typeof(Enemy)))
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
                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        break;
                    }
                    else if (entity.GetType().Equals(typeof(Enemy)))
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
                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        break;
                    }
                    else if (entity.GetType().Equals(typeof(Enemy)))
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

        int length = Mathf.Min(bounds.size.x, bounds.size.y);

        foreach (Vector3Int direction in directions)
        {
            for (int i = 1; i < length; i++)
            {
                Vector3Int moveableCellgridPosition = currentCellgridPosition + direction * i;

                if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
                {
                    Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                    if (entity != null)
                    {
                        if (entity.GetType().Equals(typeof(PlayerCharacter)))
                        {
                            break;
                        }
                        else if (entity.GetType().Equals(typeof(Enemy)))
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
