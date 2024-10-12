using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Controls controls { get; private set; }

    public bool isMouseMiddleDrag { get; private set; }
    public bool isMouseMiddleClick { get; private set; }
    public bool isMouseLeftClick { get; private set; }
    public bool isMouseRightClick { get; private set; }
    public Timer preventInputTimer { get; private set; }

    [SerializeField] private float preventInputTimeOnTurnChange;

    private void Awake()
    {
        controls = new Controls();
        preventInputTimer = new Timer(preventInputTimeOnTurnChange);
        preventInputTimer.timerAction += () => { controls.Enable(); };
        preventInputTimer.timerAction += () => { Manager.Instance.uiManager.phaseInformationUI.SetActive(false); };
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

    public void DisableInputSystemOnTurnChange()
    {
        if (Manager.Instance.gameManager.characterSelectionPhase)
        {
            if (Manager.Instance.uiManager.mercenarySlotWindow.CanProceedToBattlePhase())
            {
                controls.Disable();
                preventInputTimer.StartSingleUseTimer();
            }
        }
        else
        {
            controls.Disable();
            preventInputTimer.StartSingleUseTimer();
        }
    }
}
