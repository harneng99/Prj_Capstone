using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragCamera : MonoBehaviour
{
    private Vector3 origin;
    private Vector3 difference;

    private CinemachineVirtualCamera virtualCamera;

    private bool isDragging;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void LateUpdate()
    {
        if (isDragging)
        {
            difference = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
            transform.position = origin - difference;
        }
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            virtualCamera.Follow = null;
            origin = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        
        isDragging = context.started || context.performed;
    }
}
