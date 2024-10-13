using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerCharacter : Entity, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    private bool onInitialDeployment;
    // above variable is for checking whether current deployment is the first one.
    // it is because the player is simply going to click it to drop it on the first movement, not dragging it

    private void OnEnable()
    {
        onInitialDeployment = true;
    }

    protected override void Update()
    {
        base.Update();

        if (onInitialDeployment)
        {
            SetEntityFeetPosition((Vector2)Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition) + Vector2.down * entityCollider.bounds.extents.y * 0.2f);
            // Add offset for visual
        }
    }

    protected override void ShowInformation()
    {
        base.ShowInformation();

        /*if (UtilityFunctions.IsTilemapEmpty(highlightedTilemap))
        {
            Manager.Instance.uiManager.SetEntityInformation(entityPortrait, entityName, entityDescription, entityStat);
        }*/
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!entityMovement.MoveToGrid(GetEntityFeetPosition(), true)) // failed moving
            {
                if (entityMovement.currentWorldgridPosition.HasValue)
                {
                    entityMovement.MoveToGrid(entityMovement.currentWorldgridPosition.Value, true);
                }
                else
                {
                    Manager.Instance.uiManager.mercenarySlotWindow.ResetHighlights();
                    gameObject.SetActive(false);
                }
            }
            else // moving success
            {
                entityMovement.UpdateGridPositionData();
                Manager.Instance.uiManager.mercenarySlotWindow.OnCharacterDrop();
            }

            if (!eventData.dragging && !onInitialDeployment)
            {
                base.OnPointerClick(eventData);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
            {
                if (!onInitialDeployment)
                {
                    Manager.Instance.uiManager.mercenarySlotWindow.ReturnCharacter(gameObject);
                }
                Manager.Instance.uiManager.mercenarySlotWindow.ResetHighlights();
                entityMovement.currentWorldgridPosition = null;
                gameObject.SetActive(false);
            }
        }

        onInitialDeployment = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
        {
            highlightedTilemap.ClearAllTiles();
            Manager.Instance.gameManager.mercenaryDragging = this;
            Vector3 mousePosition = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0.0f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Manager.Instance.gameManager.mercenaryDragging = null;
    }
}
