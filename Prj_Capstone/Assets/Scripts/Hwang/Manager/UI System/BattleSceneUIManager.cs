using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BattleSceneUIManager : UIManager
{
    #region Entity Information UI
    [field: Header("Entity Information UI")]
    [field: SerializeField] public GameObject entityInformationUI { get; private set; }
    [SerializeField] private RectTransform entityInformationRectTransform;
    [SerializeField] private Image entityPortrait;
    [SerializeField] private TMP_Text entityName;
    [SerializeField] private TMP_Text entityDescription;
    [SerializeField] private TMP_Text entityHealthStatus;
    [SerializeField] private TMP_Text entityStaminaStatus;
    [SerializeField] private Slider entityHealthSlider;
    [SerializeField] private Slider entityStaminaSlider;
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
    [field: SerializeField] public RectTransform buttonGenerateRectTransform { get; private set; }
    
    [SerializeField] private GameObject endTurnButton;
    #endregion

    private void Awake()
    {
        mercenarySlotWindow = mercenarySlotWindowGameObject.GetComponent<CharacterSlotWindow>();
    }

    private void Update()
    {
        
    }

    public async void ShowWarningUI(string warningText)
    {
        if (Manager.Instance.gameManager.characterSelectionPhase && mercenarySlotWindow.CanProceedToBattlePhase())
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

    public void ShowTurnInformationUI()
    {
        if (Manager.Instance.gameManager.characterSelectionPhase)
        {
            if (mercenarySlotWindow.CanProceedToBattlePhase())
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Player Phase";
                phaseInformationUI.SetActive(true);
                entityInformationRectTransform.anchoredPosition = new Vector2(-400.0f, 0.0f);
                entityInformationRectTransform.offsetMin = Vector2.zero;
            }
        }
        else
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

    public void SetEntityInformation(Sprite entityPortrait, string entityName, string entityDescription, Stat entityStat)
    {
        entityInformationRectTransform.DOAnchorPosX(0.0f, 0.5f).SetEase(Ease.OutCubic);
        this.entityPortrait.sprite = entityPortrait;
        this.entityName.text = entityName;
        this.entityDescription.text = entityDescription;
        entityHealthStatus.text = entityStat.health.currentValue + " / " + entityStat.health.maxValue;
        entityHealthSlider.value = entityStat.health.currentValue / entityStat.health.maxValue;
        entityStaminaStatus.text = entityStat.stamina.currentValue + " / " + entityStat.stamina.maxValue;
        entityStaminaSlider.value = entityStat.stamina.currentValue / entityStat.stamina.maxValue;
    }

    public void HideEntityInformation()
    {
        entityInformationRectTransform.DOAnchorPosX(-400.0f, 0.5f).SetEase(Ease.OutCubic);
    }
}
