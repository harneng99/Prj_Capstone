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
            SetEntityFeetPosition(Manager.Instance.playerInputManager.GetMousePosition() + Vector3.down * entityCollider.bounds.extents.y * 0.2f);
            // Add offset for visual
        }
    }

    protected override void ShowInformation()
    {
        base.ShowInformation();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
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

                if (!eventData.dragging)
                {
                    base.OnPointerClick(eventData);
                }
            }
            else if (Manager.Instance.gameManager.battlePhase)
            {
                base.OnPointerClick(eventData);
            }
        }
        else if (eventData.button.Equals(PointerEventData.InputButton.Right))
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
        {
            Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(transform);
            highlightedTilemap.ClearAllTiles();
            Manager.Instance.gameManager.mercenaryDragging = this;
            transform.position = Manager.Instance.playerInputManager.GetMousePosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
        Manager.Instance.gameManager.mercenaryDragging = null;
    }
}
