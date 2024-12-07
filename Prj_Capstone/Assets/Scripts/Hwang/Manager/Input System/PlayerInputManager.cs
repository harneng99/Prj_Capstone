using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Controls controls { get; private set; }

    public bool isMouseMiddleDrag { get; private set; }
    public bool isMouseMiddleClick { get; private set; }
    public bool isMouseLeftClick { get; private set; }
    public bool isMouseRightClick { get; private set; }
    public bool isPointerOverUI { get; private set; }
    public Timer preventInputTimer { get; private set; }

    [SerializeField] private float preventInputTimeOnTurnChange;

    private void Awake()
    {
        controls = new Controls();
        preventInputTimer = new Timer(preventInputTimeOnTurnChange);
        preventInputTimer.timerAction += () => { controls.Enable(); };
        // preventInputTimer.timerAction += () => { Manager.Instance.uiManager.phaseInformationUI.SetActive(false); };
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        isMouseLeftClick = controls.Map.MouseLeftClick.WasPressedThisFrame();
        isMouseRightClick = controls.Map.MouseRightClick.WasPressedThisFrame();
        isMouseMiddleClick = controls.Map.MouseMiddleClick.WasPressedThisFrame();

        preventInputTimer.Tick();
    }

    public void OnMouseMiddleClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isMouseMiddleDrag = true;
        }

        if (context.canceled)
        {
            isMouseMiddleDrag = false;
        }
    }

    public Vector3 GetMousePosition()
    {
        return (Vector2)Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public Vector3Int GetMousePosition(GridType gridType)
    {
        Vector3Int cellgrid = Manager.Instance.gameManager.moveableTilemap.WorldToCell(GetMousePosition());

        switch (gridType)
        {
            case GridType.Hexgrid:
                return CellgridToHexgrid(cellgrid);
            case GridType.Cellgrid:
                return cellgrid;
            default:
                return default;
        }
    }

    public void DisableInputSystemOnTurnChange()
    {
        if (Manager.Instance.gameManager.pieceDeploymentPhase)
        {

            controls.Disable();
            preventInputTimer.StartSingleUseTimer();
        }
        else if (Manager.Instance.gameManager.battlePhase)
        {
            controls.Disable();
            if (Manager.Instance.gameManager.playerPhase)
            {
                preventInputTimer.StartSingleUseTimer();
            }
        }
    }

    public bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult raycastResult in raycastResults)
        {
            if (raycastResult.gameObject.layer.Equals(LayerMask.NameToLayer("UI")))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPointerOverEntity()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult raycastResult in raycastResults)
        {
            if (raycastResult.gameObject.GetComponent<Entity>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private Vector3Int CellgridToHexgrid(Vector3Int cellgridPosition)
    {
        if (cellgridPosition.y < 0 && cellgridPosition.y % 2 != 0)
        {
            return new Vector3Int(cellgridPosition.x - cellgridPosition.y / 2, cellgridPosition.y, -cellgridPosition.x - cellgridPosition.y / 2 - cellgridPosition.y % 2) + new Vector3Int(1, 0, -1);
        }
        else
        {
            return new Vector3Int(cellgridPosition.x - cellgridPosition.y / 2, cellgridPosition.y, -cellgridPosition.x - cellgridPosition.y / 2 - cellgridPosition.y % 2);
        }
    }
}
