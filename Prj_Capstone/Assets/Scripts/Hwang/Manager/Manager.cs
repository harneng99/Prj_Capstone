using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    public BattleManager gameManager { get; private set; }
    public BattleSceneUIManager uiManager { get; private set; }
    public ObjectPoolingManager objectPoolingManager { get; private set; }
    public PlayerInputManager playerInputManager { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameManager = GetComponentInChildren<BattleManager>();
        objectPoolingManager = GetComponentInChildren<ObjectPoolingManager>(); 
        uiManager = GetComponentInChildren<BattleSceneUIManager>();
        playerInputManager = GetComponentInChildren<PlayerInputManager>();
    }
}
