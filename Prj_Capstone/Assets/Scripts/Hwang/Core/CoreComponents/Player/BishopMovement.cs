using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BishopMovement : PlayerMovement
{
    private Vector3Int[] directions = { new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) };

    public override void DrawMoveableTilemap(bool showTile = true)
    {
        base.DrawMoveableTilemap(showTile);

        if (!showTile) return;

        entity.highlightedTilemap.SetTile(currentCellgridPosition, moveRangeHighlightedTileBase);

        BoundsInt bounds = pathfinder.moveableTilemap.cellBounds;

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
