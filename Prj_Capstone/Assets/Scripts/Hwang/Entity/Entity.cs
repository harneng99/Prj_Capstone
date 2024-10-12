using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour, IPointerClickHandler
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
    public event Action onPointerClick;
    public bool isSelected { get; private set; }
    public Tilemap highlightedTilemap { get; private set; }
    #endregion

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        entityCollider = GetComponent<Collider2D>();

        core = GetComponentInChildren<Core>();

        onPointerClick += ShowInformation;
        highlightedTilemap = GameObject.FindWithTag("HighlightedTilemap").GetComponent<Tilemap>();
    }

    protected virtual void Start()
    {
        entityStat = core.GetCoreComponent<Stat>();
        entityMovement = core.GetCoreComponent<Movement>();
        entityCombat = core.GetCoreComponent<Combat>();

        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }

    
    protected virtual void Update()
    {
        
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Manager.Instance.gameManager.Select(this);
        onPointerClick?.Invoke();
    }

    protected virtual void ShowInformation()
    {
        Manager.Instance.uiManager.SetEntityInformation(entityPortrait, entityName, entityDescription, entityStat);
    }

    protected void MouseRightClick()
    {
        if (isSelected)
        {
            isSelected = false;
            Manager.Instance.uiManager.HideEntityInformation();
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
