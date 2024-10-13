using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class BattleSceneUIManager : UIManager
{
    #region Information UI
    [field: Header("Information UI")]
    [field: SerializeField] public GameObject informationUI { get; private set; }
    [SerializeField] private RectTransform informationUIRectTransform;

    [SerializeField] private GameObject entityInformation;
    [SerializeField] private Image informationUIEntityPortrait;
    [SerializeField] private TMP_Text informationUIEntityName;
    [SerializeField] private Slider informationUIHealthSlider;
    [SerializeField] private TMP_Text informationUIHealthStatus;
    [SerializeField] private Slider informationUIStaminaSlider;
    [SerializeField] private TMP_Text informationUIStaminaStatus;
    [SerializeField] private TMP_Text informationUIEntityStatus;

    [SerializeField] private GameObject tileInformation;
    [SerializeField] private Image tileImage;
    [SerializeField] private TMP_Text tileInformationText;

    [SerializeField] private RectTransform shortcutPortraitGenerateRectTransform;
    [SerializeField] private float distanceBetweenShortcutPortrait;

    [field: SerializeField] public Button endTurnButton { get; private set; }
    [field: SerializeField] public RectTransform buttonGenerateRectTransform { get; private set; }
    [field: SerializeField] public Vector2 distanceBetweenCombatAbilityButtons { get; private set; }
    #endregion

    #region Side Information UI
    [field: Header("Side Information UI")]
    [field: SerializeField] public GameObject sideInformationUI { get; private set; }
    [SerializeField] private RectTransform sideInformationUIRectTransform;
    [SerializeField] private Image sideInformationUIEntityPortrait;
    [SerializeField] private TMP_Text sideInformationUIEtityName;
    [SerializeField] private TMP_Text sideInformationUIEntityStatus;
    [SerializeField] private Slider sideInformationUIEntityHealthSlider;
    [SerializeField] private TMP_Text sideInformationUIEntityHealthStatus;
    [SerializeField] private Slider sideInformationUIEntityStaminaSlider;
    [SerializeField] private TMP_Text sideInformationUIEntityStaminaStatus;
    #endregion

    #region Mercanary Slot UI
    [Header("Entity Information UI")]
    [SerializeField] private GameObject mercenarySlotWindowGameObject;
    [field: SerializeField] public CharacterSlotWindow mercenarySlotWindow { get; private set; }
    // public PlayerCharacter mercenaryDragging { get; set; }
    #endregion

    #region Warning UI
    [Header("Warning UI")]
    [SerializeField] public GameObject warningUI;
    [SerializeField] public TMP_Text warningText;
    [SerializeField] private Image warningUIBackground;
    #endregion

    #region Extra UI
    [field: Header("Extra UI")]
    [field: SerializeField] public GameObject phaseInformationUI { get; private set; }
    [field: SerializeField] public GameObject turnCounter { get; private set; }
    #endregion

    private void Awake()
    {
        mercenarySlotWindow = mercenarySlotWindowGameObject.GetComponent<CharacterSlotWindow>();
    }

    private void Start()
    {
        Manager.Instance.playerInputManager.controls.Map.MouseRightClick.performed += _ => MouseRightClick();
    }

    private void Update()
    {
        
    }

    public async void ShowWarningUI(string warningText)
    {
        if (Manager.Instance.gameManager.mercenaryDeploymentPhase && mercenarySlotWindow.CanProceedToBattlePhase())
        {
            return;
        }
    
        warningUI.SetActive(true);

        warningUIBackground.color = new Color(0.0f, 0.0f, 0.0f, 0.8f);
        this.warningText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        this.warningText.text = warningText;
        await Task.Delay(3000);

        warningUIBackground.DOFade(0.0f, 3.0f);
        this.warningText.DOFade(0.0f, 3.0f);
        await Task.Delay(3000);
        warningUI.SetActive(false);
    }

    public void ShowPhaseInformationUI()
    {
        if (Manager.Instance.gameManager.mercenaryDeploymentPhase)
        {
            if (mercenarySlotWindow.CanProceedToBattlePhase())
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Player Phase";
                phaseInformationUI.SetActive(true);
                sideInformationUIRectTransform.anchoredPosition = new Vector2(-400.0f, 0.0f);
                sideInformationUIRectTransform.offsetMin = Vector2.zero;
            }
        }
        else if (Manager.Instance.gameManager.battlePhase)
        {
            if (Manager.Instance.gameManager.playerPhase)
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Player Phase";
            }
            else if (Manager.Instance.gameManager.enemyPhase)
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Enemy Phase";
            }

            phaseInformationUI.SetActive(true);
        }
    }

    public void HideMercenaryDeployUI()
    {
        if (mercenarySlotWindow.CanProceedToBattlePhase())
        {
            foreach (GameObject mercenarySlot in mercenarySlotWindow.mercenarySlots)
            {
                mercenarySlot.SetActive(false);
            }
            mercenarySlotWindowGameObject.SetActive(false);
        }
    }

    public void SetSideInformationUI(Entity entity, string entityStatus)
    {
        // sideInformationUIRectTransform.DOAnchorPosX(0.0f, 0.5f).SetEase(Ease.OutCubic);
        sideInformationUIEntityPortrait.sprite = entity.entityPortrait;
        sideInformationUIEtityName.text = entity.entityName;
        sideInformationUIEntityStatus.text = entity.entityDescription;
        sideInformationUIEntityHealthStatus.text = entity.entityStat.health.currentValue + " / " + entity.entityStat.health.maxValue;
        sideInformationUIEntityHealthSlider.value = entity.entityStat.health.currentValue / entity.entityStat.health.maxValue;
        sideInformationUIEntityStaminaStatus.text = entity.entityStat.stamina.currentValue + " / " + entity.entityStat.stamina.maxValue;
        sideInformationUIEntityStaminaSlider.value = entity.entityStat.stamina.currentValue / entity.entityStat.stamina.maxValue;
    }

    public void SetSideInformationUI(Vector3Int cellgridPosition)
    {

    }

    public void ShowSideInformationUI()
    {
        sideInformationUI.SetActive(true);
    }

    public void HideSideInformationUI()
    {
        sideInformationUI.SetActive(false);
        // sideInformationUIRectTransform.DOAnchorPosX(sideInformationUIRectTransform.sizeDelta.x, 0.5f).SetEase(Ease.OutCubic);
    }

    public void EnableInformationUI(bool activeSelf)
    {
        informationUI.SetActive(activeSelf);
        entityInformation.SetActive(activeSelf);
        tileInformation.SetActive(activeSelf);
        buttonGenerateRectTransform.gameObject.SetActive(activeSelf);
    }

    public void GenerateShorcutPortrait()
    {
        int index = 0;

        foreach (PlayerCharacter mercenary in Manager.Instance.gameManager.mercenaries)
        {
            if (mercenary.gameObject.activeSelf)
            {
                GameObject shortcutPortraitGameObject = Manager.Instance.objectPoolingManager.GetGameObject("Shortcut Portrait");
                RectTransform shortcutPortraitRectTransform = shortcutPortraitGameObject.GetComponent<RectTransform>();
                ShortcutPortrait shortcutPortrait = shortcutPortraitGameObject.GetComponent<ShortcutPortrait>();
                shortcutPortrait.mercenary = mercenary;
                shortcutPortraitGameObject.transform.position = shortcutPortraitGenerateRectTransform.position + Vector3.right * distanceBetweenShortcutPortrait * index;
                shortcutPortraitRectTransform.SetParent(shortcutPortraitGenerateRectTransform);
                index += 1;
            }
        }
    }

    public List<GameObject> GenerateCombatAbilityButtons(Entity entity)
    {
        List<CombatAbility> combatAbilities = entity.entityCombat.combatAbilities;
        List<GameObject> combatAbilityButtons = new List<GameObject>();
        GameObject combatAbilityButtonsParent = new GameObject(entity.entityName + "'s Combat Ability Buttons");
        combatAbilityButtonsParent.transform.SetParent(buttonGenerateRectTransform);

        for (int i = 0; i < combatAbilities.Count; i++)
        {
            GameObject combatAbilityButtonPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Combat Ability Button");
            CombatAbilityButton combatAbilityButton = combatAbilityButtonPrefab.GetComponent<CombatAbilityButton>();
            RectTransform buttonRectTransform = combatAbilityButtonPrefab.GetComponent<RectTransform>();
            combatAbilityButtonPrefab.GetComponentsInChildren<Image>().Skip(1).ToList()[0].sprite = combatAbilities[i].combatAbilityIcon;

            combatAbilityButton.entity = entity;
            foreach (CombatAbilityComponent combatAbilityComponent in combatAbilities[i].combatAbilityComponents)
            {
                combatAbilityComponent.entity = entity;
            }
            combatAbilityButton.combatAbility = combatAbilities[i];
            buttonRectTransform.SetParent(combatAbilityButtonsParent.transform);
            combatAbilityButton.transform.position = buttonGenerateRectTransform.position + Vector3.right * buttonRectTransform.sizeDelta.x * 1.5f * i;
            combatAbilityButtons.Add(combatAbilityButtonPrefab);
        }

        return combatAbilityButtons;
    }

    public void SetInformationUI(Entity entity, string entityStatus, Vector3Int cellPosition)
    {
        SetEntityData(entity, entityStatus);

        SetTileData(cellPosition);
    }
     
    public void SetEntityData(Entity entity, string entityStatus)
    {
        if (entity != null)
        {
            entityInformation.SetActive(true);
            informationUIEntityPortrait.sprite = entity.entityPortrait;
            informationUIEntityName.text = entity.entityName;
            informationUIEntityStatus.text = entityStatus;
            informationUIHealthSlider.value = entity.entityStat.health.currentValue / entity.entityStat.health.maxValue;
            informationUIHealthStatus.text = entity.entityStat.health.currentValue + " / " + entity.entityStat.health.maxValue;
            informationUIStaminaSlider.value = entity.entityStat.stamina.currentValue / entity.entityStat.stamina.maxValue;
            informationUIStaminaStatus.text = entity.entityStat.stamina.currentValue + " / " + entity.entityStat.stamina.maxValue;
        }
        else
        {
            entityInformation.SetActive(false);
        }
    }

    public void SetTileData(Vector3Int cellPosition)
    {
        GameObject objectTileGameObject = Manager.Instance.gameManager.objectTilemap.GetInstantiatedObject(cellPosition);
        GameObject moveableTileGameObject = Manager.Instance.gameManager.moveableTilemap.GetInstantiatedObject(cellPosition);

        if (objectTileGameObject != null)
        {
            tileInformation.SetActive(true);
            CustomTileData customTileData = objectTileGameObject.GetComponent<CustomTileData>();
            tileImage.sprite = Manager.Instance.gameManager.objectTilemap.GetSprite(cellPosition);
            tileInformationText.text = customTileData.tileInformation;
        }
        else if (moveableTileGameObject != null)
        {
            tileInformation.SetActive(true);
            CustomTileData customTileData = moveableTileGameObject.GetComponent<CustomTileData>();
            tileImage.sprite = Manager.Instance.gameManager.moveableTilemap.GetSprite(cellPosition);
            tileInformationText.text = customTileData.tileInformation;
        }
        else
        {
            tileInformation.SetActive(false);
        }
    }

    public void ShowInformationUI()
    {
        informationUIRectTransform.DOAnchorPosY(0.0f, 0.5f).SetEase(Ease.OutCubic);
    }

    public void HideInformationUI()
    {
        informationUIRectTransform.DOAnchorPosY(-informationUIRectTransform.sizeDelta.y, 0.5f).SetEase(Ease.OutCubic);
    }

    private void MouseRightClick()
    {
        
    }

    public void SetCombatAbilityButtons()
    {
        foreach (Entity entity in Manager.Instance.gameManager.entities)
        {
            foreach (GameObject combatAbilityButton in entity.entityCombat.combatAbilityButtons)
            {
                combatAbilityButton.SetActive(entity.isSelected);
            }
        }
    }
}
