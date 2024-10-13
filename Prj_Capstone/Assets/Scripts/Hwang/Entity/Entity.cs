using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Entity Components
    [field: SerializeField] public Sprite entityPortrait { get; private set; }
    [field: SerializeField] public string entityName { get; private set; }
    [field: SerializeField, TextArea] public string entityDescription { get; private set; }
    [field: SerializeField] public int level { get; private set; } = 1;
    public Animator animator { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    public Collider2D entityCollider { get; private set; }
    public Core core { get; private set; }
    [field: SerializeField] public EntityConsistentData entityConsistentData { get; private set; }
    #endregion

    #region Core Components
    public Stat entityStat { get; private set; }
    public Movement entityMovement { get; private set; }
    public Combat entityCombat { get; private set; }
    #endregion

    #region Other Variables
    public event Action<PointerEventData> onPointerClick;
    public bool isSelected { get; private set; }
    public Tilemap highlightedTilemap { get; private set; }
    #endregion

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        entityCollider = GetComponent<Collider2D>();

        core = GetComponentInChildren<Core>();
        entityStat = core.GetCoreComponent<Stat>();
        entityMovement = core.GetCoreComponent<Movement>();
        entityCombat = core.GetCoreComponent<Combat>();

        highlightedTilemap = GameObject.FindWithTag("HighlightedTilemap").GetComponent<Tilemap>();
    }

    protected virtual void Start()
    {
        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }


    protected virtual void Update()
    {
        
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelected)
        {
            highlightedTilemap.ClearAllTiles();
        }
        if (!Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection)
        {
            Manager.Instance.gameManager.Select(this);
            ShowInformation();
        }
        onPointerClick?.Invoke(eventData);
        Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = Manager.Instance.gameManager.isAiming;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (Manager.Instance.gameManager.isAiming)
            {
                Manager.Instance.uiManager.SetSideInformationUI(this, entityDescription);
                Manager.Instance.uiManager.ShowSideInformationUI();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (Manager.Instance.gameManager.isAiming)
            {
                Manager.Instance.uiManager.HideSideInformationUI();
            }
        }
    }

    protected virtual void ShowInformation()
    {
        if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
        {
            Manager.Instance.uiManager.SetSideInformationUI(this, entityDescription);
            Manager.Instance.uiManager.ShowSideInformationUI();
        }
        else if (Manager.Instance.gameManager.battlePhase)
        {
            Manager.Instance.uiManager.SetInformationUI(this, entityDescription, entityMovement.pathfinder.moveableTilemap.WorldToCell(GetEntityFeetPosition()));
            Manager.Instance.uiManager.ShowInformationUI();
        }
    }

    protected void MouseRightClick()
    {
        if (isSelected)
        {
            isSelected = false;
            // Manager.Instance.uiManager.HideEntityInformation();
            highlightedTilemap.ClearAllTiles();
            entityCombat.currentSelectedCombatAbility = null;
        }
        Manager.Instance.gameManager.ResetEntitySelected();
    }

    public void Select() => isSelected = true;

    public void Deselect() => isSelected = false;

    /// <summary>
    /// Returns the Vector3 value of the bottom center position of the entity's collider.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEntityFeetPosition()
    {
        return new Vector3(entityCollider.bounds.center.x, entityCollider.bounds.min.y, entityCollider.bounds.center.z);
    }

    /// <summary>
    /// Gets world grid position and place the entity's feet position to it.
    /// </summary>
    /// <param name="position"></param>
    public void SetEntityFeetPosition(Vector3 position)
    {
        transform.position = position + Vector3.up * entityCollider.bounds.extents.y;
    }
}
