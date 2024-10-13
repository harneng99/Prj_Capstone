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
        if (!Manager.Instance.playerInputManager.IsPointerOverUI())
        {
            if (Manager.Instance.gameManager.battlePhase)
            {
                if (!isMoving && mercenary.isSelected)
                {
                    Vector3Int destinationCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                    /*Ray ray = Manager.Instance.gameManager.mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit rayHit;
                    if (Physics.Raycast(ray, out rayHit) && rayHit.collider.gameObject.GetComponent<Entity>() != null) return;*/

                    // if (destinationCellgridPosition.Equals(currentCellgridPosition)) return;

                    TileBase highlightedTile = mercenary.highlightedTilemap.GetTile(destinationCellgridPosition);

                    if (highlightedTile != null && highlightedTile.Equals(moveRangeHighlightedTileBase))
                    {
                        if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                        {
                            mercenary.highlightedTilemap.ClearAllTiles();
                        }
                    }
                }
            }
        }
    }
}
