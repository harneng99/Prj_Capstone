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
                    if (mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Paralyze) || mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Stun) || mercenary.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Horror))
                    {
                        Manager.Instance.uiManager.ShowWarningUI("Warning: Selected mercenary can't move.");
                        return;
                    }

                    Vector3Int destinationCellgridPosition = Manager.Instance.playerInputManager.GetMousePosition(GridType.Cellgrid);

                    TileBase highlightedTile = mercenary.highlightedTilemap.GetTile(destinationCellgridPosition);

                    if (highlightedTile != null && highlightedTile.Equals(moveRangeHighlightedTileBase))
                    {
                        if (MoveToGrid(destinationCellgridPosition, GridType.Cellgrid, false))
                        {
                            DrawMoveableTilemap(false);
                        }
                    }
                }
            }
        }
    }
}
