using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PromotionCanvas : MonoBehaviour
{
    private Player player;
    private Canvas canvas;
    private Button[] promotionButtons;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        player = GetComponentInParent<Player>();
        promotionButtons = canvas.gameObject.GetComponentsInChildren<Button>();

        promotionButtons[0].onClick.AddListener(() => player.playerMovement.ChangePieceType(PieceType.Knight));
        promotionButtons[0].onClick.AddListener(() => canvas.gameObject.SetActive(false));

        promotionButtons[1].onClick.AddListener(() => player.playerMovement.ChangePieceType(PieceType.Bishop));
        promotionButtons[1].onClick.AddListener(() => canvas.gameObject.SetActive(false));

        promotionButtons[2].onClick.AddListener(() => player.playerMovement.ChangePieceType(PieceType.Rook));
        promotionButtons[2].onClick.AddListener(() => canvas.gameObject.SetActive(false));

        promotionButtons[3].onClick.AddListener(() => player.playerMovement.ChangePieceType(PieceType.Queen));
        promotionButtons[3].onClick.AddListener(() => canvas.gameObject.SetActive(false));

        promotionButtons.Last().onClick.AddListener(() => canvas.gameObject.SetActive(false));
        promotionButtons.Last().onClick.AddListener(() => { Manager.Instance.uiManager.endTurnButton.interactable = true; });
    }
}
