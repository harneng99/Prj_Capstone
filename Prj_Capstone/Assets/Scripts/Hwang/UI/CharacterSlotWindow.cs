using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlotWindow : MonoBehaviour, IPointerExitHandler, IDropHandler
{
    [SerializeField] private List<GameObject> mercenariesList;
    [SerializeField] private RectTransform mercenarySlotInitialRectTransform;
    [SerializeField] private float distanceBetweenMercenarySlot;
    [SerializeField] private float mercenarySlotMoveDurationTime;

    public CharacterSlot selectedMercenarySlot { get; set; }
    public bool containsMousePointer { get; private set; }

    private RectTransform mercenarySlotBackgroundRectTransform;
    public List<GameObject> mercenarySlots { get; private set; } = new List<GameObject>();
    private int initialMercenaryCount;

    private void Awake()
    {
        // TODO: Get character pool and information from save data and instantiate characters with it
        
        mercenarySlotBackgroundRectTransform = GetComponent<RectTransform>();

        initialMercenaryCount = mercenariesList.Count;
        for (int index = 0; index < mercenariesList.Count; index++)
        {
            GameObject mercenarySlotPrefab = Manager.Instance.objectPoolingManager.GetGameObject("Character Slot");
            GameObject mercenaryGameObject = Instantiate(mercenariesList[index]);
            mercenaryGameObject.SetActive(false);
            RectTransform mercenarySlotRectTransform = mercenarySlotPrefab.GetComponent<RectTransform>();
            CharacterSlot mercenarySlot = mercenarySlotPrefab.GetComponentInChildren<CharacterSlot>();

            mercenarySlot.SetSlot(index, mercenaryGameObject);
            mercenarySlotRectTransform.SetParent(mercenarySlotBackgroundRectTransform);
            mercenarySlotRectTransform.position = mercenarySlotInitialRectTransform.position + index * Vector3.right * distanceBetweenMercenarySlot;
            
            mercenarySlots.Add(mercenarySlotPrefab);
        }
    }

    private void Update()
    {
        containsMousePointer = RectTransformUtility.RectangleContainsScreenPoint(mercenarySlotBackgroundRectTransform, Input.mousePosition);

        if (selectedMercenarySlot != null)
        {
            selectedMercenarySlot.assignedMercenary.SetActive(!containsMousePointer);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        /*if (selectedCharacterSlot != null)
        {
            selectedCharacterSlot.gameObject.SetActive(false);
            GameObject selectedCharacter = Instantiate(selectedCharacterSlot.characterGameObjectPrefab);
            Vector3 mousePosition = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            selectedCharacter.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0.0f);
        }*/
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (mercenarySlots[0].GetComponent<RectTransform>().position.x > eventData.position.x)
        {
            mercenarySlots.Insert(0, selectedMercenarySlot.gameObject);
            RearrangeSlotPositions(0, true);
        }
        else if (mercenarySlots[mercenarySlots.Count - 1].GetComponent<RectTransform>().position.x <= eventData.position.x)
        {
            mercenarySlots.Add(selectedMercenarySlot.gameObject);
            RearrangeSlotPositions(mercenarySlots.Count - 1, true);
        }
        else
        {
            for (int index = 0; index < mercenarySlots.Count - 1; index++)
            {
                RectTransform leftMercenarySlotRectTransform = mercenarySlots[index].GetComponent<RectTransform>();
                RectTransform rightMercenarySlotRectTransform = mercenarySlots[index + 1].GetComponent<RectTransform>();

                if (leftMercenarySlotRectTransform.position.x <= eventData.position.x && eventData.position.x < rightMercenarySlotRectTransform.position.x)
                {
                    mercenarySlots.Insert(index + 1, selectedMercenarySlot.gameObject);
                    RearrangeSlotPositions(index + 1, true);
                    break;
                }
            }
        }

        selectedMercenarySlot = null;
    }

    public void CharacterSlotSelected(CharacterSlot selectedMercenarySlot)
    {
        if (this.selectedMercenarySlot != null && this.selectedMercenarySlot.Equals(selectedMercenarySlot))
        {
            selectedMercenarySlot.selectedHighlight.SetActive(false);
            this.selectedMercenarySlot = null;
        }
        else
        {
            ResetHighlights();
            this.selectedMercenarySlot = selectedMercenarySlot;
            selectedMercenarySlot.selectedHighlight.SetActive(true);
        }
    }

    public void ResetHighlights()
    {
        foreach (GameObject mercenarySlotGameObject in mercenarySlots)
        {
            CharacterSlot mercenarySlot = mercenarySlotGameObject.GetComponent<CharacterSlot>();
            mercenarySlot.selectedHighlight.SetActive(false);
        }
        selectedMercenarySlot = null;
    }

    public void OnCharacterDrop()
    {
        if (selectedMercenarySlot == null) return;

        mercenarySlots.RemoveAt(selectedMercenarySlot.slotIndex);

        for (int index = selectedMercenarySlot.slotIndex; index < mercenarySlots.Count; index++)
        {
            CharacterSlot mercenarySlot = mercenarySlots[index].GetComponent<CharacterSlot>();
            RectTransform mercenarySlotRectTransform = mercenarySlots[index].GetComponent<RectTransform>();

            mercenarySlot.slotIndex = index;
            mercenarySlotRectTransform.DOMove(mercenarySlotInitialRectTransform.position + index * Vector3.right * distanceBetweenMercenarySlot, mercenarySlotMoveDurationTime);
        }

        selectedMercenarySlot.ReleaseObject();
        selectedMercenarySlot = null;
    }

    public void ReturnCharacter(GameObject returnedObject)
    {
        GameObject mercenarySlotGameObject = Manager.Instance.objectPoolingManager.GetGameObject("Character Slot");
        mercenarySlotGameObject.transform.SetParent(mercenarySlotBackgroundRectTransform);
        CharacterSlot characterSlot = mercenarySlotGameObject.GetComponent<CharacterSlot>();
        characterSlot.SetSlot(mercenarySlots.Count, returnedObject);
        mercenarySlots.Add(mercenarySlotGameObject);
        mercenarySlotGameObject.GetComponent<RectTransform>().position = mercenarySlotInitialRectTransform.position + (mercenariesList.Count - 1) * Vector3.right * distanceBetweenMercenarySlot;
        RearrangeSlotPositions(0, true);
    }

    private void RearrangeSlotPositions(int startIndex, bool slotDrop)
    {
        if (slotDrop)
        {
            for (int index = startIndex; index < mercenarySlots.Count; index++)
            {
                CharacterSlot mercenarySlot = mercenarySlots[index].GetComponent<CharacterSlot>();
                RectTransform mercenarySlotRectTransform = mercenarySlots[index].GetComponent<RectTransform>();

                mercenarySlot.slotIndex = index;
                mercenarySlotRectTransform.DOMove(mercenarySlotInitialRectTransform.position + index * Vector3.right * distanceBetweenMercenarySlot, mercenarySlotMoveDurationTime);
            }
        }
        else
        {

        }
    }

    public bool CanProceedToBattlePhase()
    {
        return mercenarySlots.Count < initialMercenaryCount;
    }
}
