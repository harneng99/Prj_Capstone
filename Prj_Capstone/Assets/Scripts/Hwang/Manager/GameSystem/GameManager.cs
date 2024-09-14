using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Camera mainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
}
