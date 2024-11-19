using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    public GameManager gameManager { get; private set; }
    public UIManager uiManager { get; private set; }
    public ObjectPoolingManager objectPoolingManager { get; private set; }
    public PlayerInputManager playerInputManager { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameManager = GetComponentInChildren<GameManager>();
        objectPoolingManager = GetComponentInChildren<ObjectPoolingManager>(); 
        uiManager = GetComponentInChildren<UIManager>();
        playerInputManager = GetComponentInChildren<PlayerInputManager>();
    }
}
