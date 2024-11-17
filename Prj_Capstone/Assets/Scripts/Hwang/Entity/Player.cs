using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class Player : Entity, IPointerClickHandler
{
    public PlayerMovement playerMovement { get; private set; }
    public PlayerCombat playerCombat { get; private set; }
    public Canvas canvas { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        playerMovement = entityMovement as PlayerMovement;
        playerCombat = entityCombat as PlayerCombat;
    }

    protected override void Start()
    {
        base.Start();

        canvas = GetComponentInChildren<Canvas>(true);

        if (canvas != null)
        {
            Button[] promotionButtons = canvas.gameObject.GetComponentsInChildren<Button>();

            // TODO: (PieceType)index in for loop does not seems to work. Why?
            /*promotionButtons[0].onClick.AddListener(() => entityMovement.ChangePieceType(PieceType.Knight));
            promotionButtons[0].onClick.AddListener(() => canvas.gameObject.SetActive(false));
            promotionButtons[1].onClick.AddListener(() => entityMovement.ChangePieceType(PieceType.Bishop));
            promotionButtons[1].onClick.AddListener(() => canvas.gameObject.SetActive(false));
            promotionButtons[2].onClick.AddListener(() => entityMovement.ChangePieceType(PieceType.Rook));
            promotionButtons[2].onClick.AddListener(() => canvas.gameObject.SetActive(false));
            promotionButtons[3].onClick.AddListener(() => entityMovement.ChangePieceType(PieceType.Queen));
            promotionButtons[3].onClick.AddListener(() => canvas.gameObject.SetActive(false));*/

            for (int i = 0; i < promotionButtons.Count() - 1; i++)
            {
                promotionButtons[i].onClick.AddListener(() => entityMovement.ChangePieceType((PieceType)(i + 1)));
                promotionButtons[i].onClick.AddListener(() => canvas.gameObject.SetActive(false));
            }

            promotionButtons.Last().onClick.AddListener(() => canvas.gameObject.SetActive(false));
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase)
            {
                base.OnPointerClick(eventData);

                // TODO: Allow promotion only after movement?
                if (entityMovement.pieceType == PieceType.Pawn && Manager.Instance.gameManager.didEntityMovedThisTurn)
                {
                    CustomTileData currentTileData = interactableTilemap.GetInstantiatedObject(entityMovement.currentCellgridPosition)?.GetComponent<CustomTileData>();

                    if (currentTileData != null && currentTileData.interactableTileLayer == InteractableTileLayer.Promotion)
                    {
                        canvas.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public void PieceAbility()
    {
        playerMovement.PieceAbility();
    }

    public override int Flip(float? direction = null)
    {
        base.Flip(direction);

        if (canvas != null)
        {
            if (facingDirection == 1)
            {
                canvas.transform.rotation = Quaternion.identity;
            }
            else
            {
                canvas.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            }
        }

        return facingDirection;
    }
}
