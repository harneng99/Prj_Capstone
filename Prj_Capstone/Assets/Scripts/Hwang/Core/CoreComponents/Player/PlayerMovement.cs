using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerMovement : Movement
{
    PlayerCharacter playerCharacter;

    protected override void Awake()
    {
        base.Awake();

        playerCharacter = entity as PlayerCharacter;
    }

    protected override void MouseLeftClick()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (!isMoving && entity.isSelected)
            {
                Manager.Instance.uiManager.HideEntityInformation();
                Vector3Int destinationCellgridPosition = pathfinder.moveableTilemap.WorldToCell(Manager.Instance.playerInputManager.GetMousePosition());
                TileBase highlightedTile = entity.highlightedTilemap.GetTile(destinationCellgridPosition);
                entity.highlightedTilemap.ClearAllTiles();
                isShowingMoveableTiles = false;

                if (highlightedTile != null)
                {
                    MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false);
                }
            }
        }
    }
}
