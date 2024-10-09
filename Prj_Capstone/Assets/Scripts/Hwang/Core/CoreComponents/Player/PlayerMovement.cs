using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerMovement : Movement
{
    PlayerCharacter mercenary;

    protected override void Awake()
    {
        base.Awake();

        mercenary = entity as PlayerCharacter;
    }

    protected override void Start()
    {
        base.Start();

        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();
    }

    // This is called whenever the mouse clicks anywhere. It is different from OnPointerClick event function.
    private void MouseLeftClick()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (!isMoving && mercenary.isSelected)
            {
                Manager.Instance.uiManager.HideEntityInformation();
                Vector3Int destinationCellgridPosition = pathfinder.moveableTilemap.WorldToCell(Manager.Instance.playerInputManager.GetMousePosition());
                TileBase highlightedTile = mercenary.highlightedTilemap.GetTile(destinationCellgridPosition);
                mercenary.highlightedTilemap.ClearAllTiles();
                isShowingMoveableTiles = false;

                if (highlightedTile != null && highlightedTile.Equals(moveRangeHighlightedTileBase))
                {
                    MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false);
                }
            }
        }
    }
}
