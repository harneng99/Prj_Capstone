using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public Entity currentSelectedEntity;
    [field: SerializeField] public Camera mainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
    public int currentTurnCount { get; private set; }

    public event Action playerTurnStart;
    public event Action playerTurnEnd;
    public event Action enemyTurnStart;
    public event Action enemyTurnEnd;
    
    public event Action entitySelected;

    private List<Entity> entities;

    private void Awake()
    {
        entities = FindObjectsOfType<Entity>().ToList();

        playerTurnStart += PlayerStaminaRecovery;
    }

    public void ResetEntitySelected()
    {
        foreach (Entity entity in entities)
        {
            entity.isSelected = false;
        }
    }

    public void PlayerTurnStart()
    {
        playerTurnStart?.Invoke();
    }

    private void PlayerStaminaRecovery()
    {
        foreach (Entity entity in entities)
        {
            entity.entityStat.stamina.IncreaseCurrentValue(entity.entityStat.stamina.recoveryValue);
        }
    }
}
