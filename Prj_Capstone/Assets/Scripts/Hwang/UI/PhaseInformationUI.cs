using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseInformationUI : MonoBehaviour
{
    [field: SerializeField] public float uiDuration { get; private set; } = 2.0f;

    private void OnEnable()
    {
        Invoke("DisableGameObject", uiDuration);
    }

    private void DisableGameObject()
    {
        gameObject.SetActive(false);
        if (Manager.Instance.gameManager.playerPhase)
        {
            Manager.Instance.uiManager.endTurnButton.interactable = true;
        }
    }
}
