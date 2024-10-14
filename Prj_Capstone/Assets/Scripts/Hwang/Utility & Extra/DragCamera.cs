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
            difference = Manager.Instance.playerInputManager.GetMousePosition() - transform.position;
            Manager.Instance.gameManager.virtualCameraFollowTransform.position = origin - difference;
        }
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // virtualCamera.Follow = null;
            Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
            origin = Manager.Instance.playerInputManager.GetMousePosition();
        }
        
        isDragging = context.started || context.performed;
    }
}
