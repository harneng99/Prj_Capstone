using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Controls controls { get; private set; }

    public bool isMouseMiddleDrag;
    public bool isMouseMiddleClick;
    public bool isMouseLeftClick;
    public bool isMouseRightClick;

    private void Awake()
    {
        controls = new Controls();
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
}
