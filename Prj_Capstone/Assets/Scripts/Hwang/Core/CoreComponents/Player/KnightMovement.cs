using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnightMovement : PlayerMovement
{
    public override void DrawMoveableTilemap(bool showTile = true)
    {
        base.DrawMoveableTilemap(showTile);

        if (!showTile) return;

        // entity.highlightedTilemap.SetTile(currentCellgridPosition, moveRangeHighlightedTileBase);

        foreach (Vector3Int movement in knightMovements)
        {
            Vector3Int moveableCellgridPosition = currentCellgridPosition + movement;

            if (pathfinder.moveableTilemap.HasTile(moveableCellgridPosition))
            {
                Entity entity = Manager.Instance.gameManager.EntityExistsAt(moveableCellgridPosition);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(PlayerCharacter)))
                    {
                        continue;
                    }
                    else if (entity.GetType().Equals(typeof(Enemy)))
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
}
