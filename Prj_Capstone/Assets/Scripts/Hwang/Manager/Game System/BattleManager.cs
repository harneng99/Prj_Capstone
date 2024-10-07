using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    [field: SerializeField] public Camera mainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
    public Entity currentSelectedEntity { get; private set; }
    public bool characterSelectionPhase { get; private set; } = true;
    public bool battlePhase { get; private set; } = false;
    public bool playerPhase { get; private set; } = false;
    public bool enemyPhase { get; private set; } = false;
    public int currentTurnCount { get; private set; }
    public Tilemap moveableTilemap { get; private set; }
    public Tilemap objectTilemap { get; private set; }
    public Tilemap selectionTilemap { get; private set; }
    public Tilemap fogTilemap { get; private set; }
    public PlayerCharacter mercenaryDragging { get; set; }

    public Vector3Int currentMouseCellgridPosition { get; private set; }
    public List<Entity> entities { get; private set; } = new List<Entity>();
    private List<PlayerCharacter> mercenaries = new List<PlayerCharacter>();
    private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private TileBase selectionTile;

    public event Action playerTurnStart;
    public event Action playerTurnEnd;
    public event Action enemyTurnStart;
    public event Action enemyTurnEnd;

    private void Awake()
    {
        moveableTilemap = GameObject.FindWithTag("MoveableTilemap").GetComponent<Tilemap>();
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();
        selectionTilemap = GameObject.FindWithTag("SelectionTilemap").GetComponent<Tilemap>();
        // fogTilemap = GameObject.FindWithTag("FogTilemap").GetComponent<Tilemap>();

        playerTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(PlayerCharacter)); };
        playerTurnStart += Manager.Instance.playerInputManager.DisableInputSystemOnTurnChange;
        playerTurnStart += () => { currentTurnCount += 1; };
        enemyTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(Enemy)); };
        enemyTurnStart += Manager.Instance.playerInputManager.DisableInputSystemOnTurnChange;
    }

    private void Update()
    {
        Vector3 mousePosition = Manager.Instance.gameManager.mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector3Int nextCellgridPosition;

        if (mercenaryDragging != null)
        {
            nextCellgridPosition = selectionTilemap.WorldToCell(mercenaryDragging.GetEntityFeetPosition());
        }
        else
        {
            nextCellgridPosition = selectionTilemap.WorldToCell(mousePosition);
        }
        nextCellgridPosition = new Vector3Int(nextCellgridPosition.x, nextCellgridPosition.y, 0);

        if (currentMouseCellgridPosition != nextCellgridPosition)
        {
            currentMouseCellgridPosition = nextCellgridPosition;
            selectionTilemap.ClearAllTiles();

            if (!OutOfRange(currentMouseCellgridPosition))
            {
                selectionTilemap.SetTile(nextCellgridPosition, selectionTile);
            }
        }
    }

    public void Select(Entity entity)
    {
        ResetEntitySelected();
        entity.Select();
        currentSelectedEntity = entity;
        virtualCamera.Follow = entity.transform;
    }

    public void ResetEntitySelected()
    {
        foreach (Entity entity in entities)
        {
            entity.Deselect();
        }
    }

    public void PlayerTurnStart()
    {
        if (Manager.Instance.gameManager.characterSelectionPhase && Manager.Instance.uiManager.mercenarySlotWindow.CanProceedToBattlePhase())
        {
            enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
            entities = FindObjectsByType<Entity>(FindObjectsSortMode.None).ToList();
            mercenaries = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None).ToList();
            foreach (Entity entity in entities)
            {
                entity.highlightedTilemap.ClearAllTiles();
            }
            playerTurnStart?.Invoke();
            characterSelectionPhase = false;
            battlePhase = true;
            playerPhase = true;
            currentTurnCount += 1;
        }
    }

    public void TurnEnd()
    {
        if (playerPhase)
        {
            playerPhase = false;
            playerTurnEnd?.Invoke();
            enemyPhase = true;
            enemyTurnStart?.Invoke();
        }
        else if (enemyPhase)
        {
            enemyPhase = false;
            enemyTurnEnd?.Invoke();
            playerPhase = true;
            playerTurnStart?.Invoke();
        }
    }

    private void EntityStatsRecovery(Type entityType)
    {
        foreach (Entity entity in entities)
        {
            if (entity.GetType().Equals(entityType))
            {
                // TODO: Find a way to do this in code, not manually
                entity.entityStat.health.IncreaseCurrentValue(entity.entityStat.health.recoveryValue);
                entity.entityStat.stamina.IncreaseCurrentValue(entity.entityStat.stamina.recoveryValue);
            }
        }
    }

    public bool OutOfRange(Vector3Int cellgridPosition)
    {
        bool inMoveable = moveableTilemap.HasTile(cellgridPosition);
        bool inObject = objectTilemap.HasTile(cellgridPosition);

        return !inMoveable && !inObject;
    }
}
