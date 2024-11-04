using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class RookMovement : PlayerMovement
{
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
    }
}
