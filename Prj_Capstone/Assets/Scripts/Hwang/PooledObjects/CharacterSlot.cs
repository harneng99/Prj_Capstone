using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : PooledObject, IPointerClickHandler
{
    [field: SerializeField] public GameObject selectedHighlight { get; private set; }
    public Image portrait { get; private set; }
    public TMP_Text characterName { get; private set; }
    public int slotIndex { get; set; }
    public GameObject assignedMercenary { get; private set; } // rather change it to script component if possible
    
    private void Awake()
    {
        portrait = UtilityFunctions.GetComponentInChildren<Image>(gameObject, true, false);
        characterName = GetComponentInChildren<TMP_Text>();
    }

    public void SetSlot(int slotIndex, GameObject assignedCharacter)
    {
        this.slotIndex = slotIndex;
        this.assignedMercenary = assignedCharacter;
        portrait.sprite = assignedCharacter.GetComponent<PlayerCharacter>().entityPortrait;
        characterName.text = assignedCharacter.GetComponent<PlayerCharacter>().entityName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Manager.Instance.uiManager.mercenarySlotWindow.CharacterSlotSelected(this);
    }
}
