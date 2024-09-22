using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour, IPointerClickHandler
{
    #region Entity Components
    [SerializeField] private Sprite portrait;
    public Animator animator { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    public Collider2D entityCollider { get; private set; }
    public Core core { get; private set; }
    public InputHandler inputHandler { get; private set; }
    [field: SerializeField] public EntityConsistentData entityConsistentData { get; private set; }
    #endregion

    #region Core Components
    public Stat entityStat { get; private set; }
    public Movement entityMovement { get; private set; }
    public Combat entityCombat { get; private set; }
    #endregion

    #region Other Variables
    public event Action onPointerClick;
    [HideInInspector] public bool isSelected;
    #endregion

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        entityCollider = GetComponent<Collider2D>();
        inputHandler = GetComponent<InputHandler>();

        core = GetComponentInChildren<Core>();

        onPointerClick += ShowInformation;
    }

    protected virtual void Start()
    {
        entityStat = core.GetCoreComponent<Stat>();
        entityMovement = core.GetCoreComponent<Movement>();
        entityCombat = core.GetCoreComponent<Combat>();
    }

    
    protected virtual void Update()
    {
        if (inputHandler.isMouseRightClick)
        {
            isSelected = false;
            Manager.Instance.gameManager.ResetEntitySelected();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClick?.Invoke();
        Manager.Instance.gameManager.ResetEntitySelected();
        Manager.Instance.gameManager.currentSelectedEntity = this;
        isSelected = true;
    }

    private void ShowInformation()
    {
        isSelected = true;
        Manager.Instance.gameManager.virtualCamera.Follow = transform;
        Debug.Log("Show Informations");
    }

    public Vector3 GetEntityFeetPosition()
    {
        return new Vector3(entityCollider.bounds.center.x, entityCollider.bounds.min.y, entityCollider.bounds.center.z);
    }
}
