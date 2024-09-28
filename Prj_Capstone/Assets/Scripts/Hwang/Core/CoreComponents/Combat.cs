using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Combat : CoreComponent
{
    [field: SerializeField] public Tilemap combatAbilityRangeHighlightedTilemap { get; private set; }
    [field: SerializeField] public TileBase combatAbilityRangeHighlightedTileBase { get; private set; }
    [field: SerializeField] public TileBase combatAbilityAOEHighlightedTileBase { get; private set; }
    
    [SerializeField] private List<CombatAbility> combatAbilities;
    [SerializeField] private RectTransform buttonGenerateRectTransform;

    [HideInInspector] public CombatAbility currentSelectedCombatAbility;

    private List<GameObject> combatAbilityButtons;
    
    private Canvas canvas;

    protected override void Awake()
    {
        base.Awake();

        entity.onPointerClick += GenerateCombatAbilityButtons;
        canvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        entity.inputHandler.controls.Map.MouseLeftClick.performed += _ => MouseClick();
    }

    private void Update()
    {
        if (entity.isSelected && currentSelectedCombatAbility != null)
        {
            Vector2 mousePosition = entity.inputHandler.controls.Map.MousePosition.ReadValue<Vector2>();
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3Int selectedCellgridPosition = entity.entityMovement.pathfinder.moveableTilemap.WorldToCell(mousePosition);

            // TODO: Draw Area of Effect tile
        }
    }

    private void MouseClick()
    {
        // TODO: Apply combat ability components to entities in area of effect
    }

    private void GenerateCombatAbilityButtons()
    {
        if (!entity.isSelected)
        {
            for (int i = 0; i < combatAbilities.Count; i++)
            {
                GameObject combatAbilityButtonPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Button");
                CombatAbilityButton combatAbilityButton = combatAbilityButtonPrefab.GetComponent<CombatAbilityButton>();
                RectTransform buttonRectTransform = combatAbilityButtonPrefab.GetComponent<RectTransform>();
                combatAbilityButtonPrefab.GetComponentsInChildren<Image>().Skip(1).ToList()[0].sprite = combatAbilities[i].combatAbilityIcon;
                
                combatAbilityButton.entity = entity;
                combatAbilityButton.combatAbility = combatAbilities[i];
                buttonRectTransform.SetParent(canvas.transform);
                Vector3 currentLocalPosition = buttonGenerateRectTransform.localPosition + Vector3.right * buttonRectTransform.sizeDelta.x * 1.5f * i;
                buttonRectTransform.localPosition = currentLocalPosition;
            }
        }
    }
    
    // TODO: Delete all buttons when mouse right clicked
    // Done by unity event or calling individual functions?
}
