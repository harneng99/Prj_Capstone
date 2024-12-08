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

    [SerializeField] private AudioClip abilitySound;

    protected override void Awake()
    {
        base.Awake();

        playerMovement = entityMovement as PlayerMovement;
        playerCombat = entityCombat as PlayerCombat;

        canvas = GetComponentInChildren<Canvas>(true);
    }

    protected override void Start()
    {
        base.Start();

        if (canvas != null)
        {
            if (facingDirection != 1)
            {
                canvas.transform.Rotate(0.0f, 180.0f, 0.0f);
                canvas.transform.localPosition = new Vector3(-canvas.transform.localPosition.x, canvas.transform.localPosition.y, canvas.transform.localPosition.z);
            }
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
        Manager.Instance.soundFXManager.PlaySoundFXClip(abilitySound, transform);
        playerMovement.PieceAbility();
    }

    protected override void FlipCanvas()
    {
        if (canvas != null)
        {
            canvas.transform.Rotate(0.0f, 180.0f, 0.0f);
            canvas.transform.localPosition = new Vector3(-canvas.transform.localPosition.x, canvas.transform.localPosition.y, canvas.transform.localPosition.z);
        }
    }
}
