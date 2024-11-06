using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    [field: SerializeField] public Camera mainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
    [field: SerializeField] public Transform virtualCameraFollowTransform { get; private set; }
    [field: SerializeField] public int howManyShouldBeInTheGoal { get; private set; }
    [field: SerializeField] public int turnLimit { get; private set; }
    public int howManyCurrentInGoal { get; set; }
    public Entity prevSelectedEntity { get; private set; }
    public Entity currentSelectedEntity { get; private set; }
    public bool pieceDeploymentPhase { get; private set; } = true;
    public bool battlePhase { get; private set; } = false;
    public bool playerPhase { get; private set; } = false;
    public bool didPlayerMovedAnythingThisTurn { get; set; }
    public bool gameFinished { get; set; }
    public bool enemyPhase { get; private set; } = false;
    public bool isAiming { get; set; } = false;
    public bool isAimingCopyForFunctionExecutionOrderCorrection { get; set; } = false; // TODO: A better way to implement this
    public int currentTurnCount { get; private set; }
    public Tilemap moveableTilemap { get; private set; }
    public Tilemap objectTilemap { get; private set; }
    public Tilemap selectionTilemap { get; private set; }
    public Tilemap highlightedTilemap { get; private set; }
    public Tilemap fogTilemap { get; private set; }
    public PlayerCharacter mercenaryDragging { get; set; }

    [field: SerializeField] public TileBase selectionTile { get; private set; }
    public Vector3Int currentMouseCellgridPosition { get; private set; }
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<PlayerCharacter> mercenaries { get; private set; } = new List<PlayerCharacter>();
    public List<Enemy> enemies { get; private set; } = new List<Enemy>();

    public event Action playerTurnStart;
    public event Action playerTurnEnd;
    public event Action enemyTurnStart;
    public event Action enemyTurnEnd;

    private Coroutine enemyAttackCoroutine;
    public bool iterateNextEnemy { get; set; }
    public bool continueTurn { get; set; }

    private void Awake()
    {
        moveableTilemap = GameObject.FindWithTag("MoveableTilemap").GetComponent<Tilemap>();
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();
        selectionTilemap = GameObject.FindWithTag("SelectionTilemap").GetComponent<Tilemap>();
        highlightedTilemap = GameObject.FindWithTag("HighlightedTilemap").GetComponent<Tilemap>();
        // fogTilemap = GameObject.FindWithTag("FogTilemap").GetComponent<Tilemap>();

        playerTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(PlayerCharacter)); };
        playerTurnStart += () => { currentTurnCount += 1; };
        playerTurnStart += () => { Manager.Instance.uiManager.turnCounter.GetComponent<TMP_Text>().text = "Turn " + currentTurnCount; };
        playerTurnStart += () => { Manager.Instance.uiManager.endTurnButton.interactable = true; };
        playerTurnStart += () =>
        { 
            didPlayerMovedAnythingThisTurn = false;
            foreach (Entity entity in entities)
            {
                entity.entityMovement.ResetEntityMovedBooleanVariable();
            }
        };

        // TODO: Below code deletes all the ClickHandler. Should find a way to fix this.
        // playerTurnEnd += Manager.Instance.playerInputManager.DisableInputSystemOnTurnChange;
        playerTurnEnd += Manager.Instance.uiManager.HideInformationUI;
        playerTurnEnd += Manager.Instance.uiManager.ShowPhaseInformationUI;

        enemyTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(Enemy)); };
        enemyTurnStart += () => { highlightedTilemap.ClearAllTiles(); };
        // enemyTurnStart += () => { RunEnemyAI(); };
        enemyTurnStart += () =>
        {
            if (enemyAttackCoroutine != null)
            {
                StopCoroutine(enemyAttackCoroutine);
            }
            StartCoroutine(EnemyAttack());
        };
        enemyTurnStart += () => { Manager.Instance.uiManager.endTurnButton.interactable = false; };

        enemyTurnEnd += Manager.Instance.uiManager.ShowPhaseInformationUI;
    }

    private void Start()
    {
        Manager.Instance.playerInputManager.controls.Map.MouseLeftClick.performed += _ => MouseLeftClick();

        StartBattlePhase();
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

    private void MouseLeftClick()
    {
        if (battlePhase)
        {
            Ray ray = Manager.Instance.gameManager.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit))
            {
                if (rayHit.collider.gameObject.GetComponent<CustomTileData>() != null)
                {
                    if (currentSelectedEntity == null)
                    {
                        Manager.Instance.uiManager.SetInformationUI(null, null, currentMouseCellgridPosition);
                    }
                    else
                    {
                        Manager.Instance.uiManager.SetTileData(currentMouseCellgridPosition);
                    }
                    Manager.Instance.uiManager.ShowInformationUI();
                }
            }
        }
    }

    public void Select(Entity entity)
    {
        prevSelectedEntity = currentSelectedEntity;
        ResetEntitySelected();
        entity.Select();
        currentSelectedEntity = entity;
        Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(entity.transform);
    }

    public void ResetEntitySelected()
    {
        currentSelectedEntity = null;
        foreach (Entity entity in entities)
        {
            entity.Deselect();
        }
    }

    public void StartBattlePhase()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        entities = FindObjectsByType<Entity>(FindObjectsSortMode.None).ToList();
        mercenaries = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None).ToList();
        /*foreach (Entity entity in entities)
        {
            entity.highlightedTilemap.ClearAllTiles();
        }*/
        Manager.Instance.uiManager.turnCounter.SetActive(true);

        playerTurnStart?.Invoke();
        ResetEntitySelected();
        pieceDeploymentPhase = false;
        battlePhase = true;
        playerPhase = true;
        enemyPhase = false;
        Manager.Instance.uiManager.ShowPhaseInformationUI();
        Manager.Instance.uiManager.HideSideInformationUI();
    }

    public async void TurnEnd()
    {
        if (continueTurn) return;

        highlightedTilemap.ClearAllTiles();

        if (playerPhase && currentTurnCount >= turnLimit && !gameFinished)
        {
            Manager.Instance.uiManager.ShowGameResultWindow("Stage Failed...");
            gameFinished = true;
        }

        if (playerPhase && !gameFinished)
        {
            playerPhase = false;
            enemyPhase = true;
            
            playerTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            enemyTurnStart?.Invoke();
        }
        else if (enemyPhase && !gameFinished)
        {
            enemyPhase = false;
            playerPhase = true;
            
            enemyTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            playerTurnStart?.Invoke();
        }
    }

    public async void TurnEndButton()
    {
        continueTurn = false;

        if (playerPhase && !gameFinished)
        {
            playerPhase = false;
            enemyPhase = true;
            
            playerTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            enemyTurnStart?.Invoke();
        }
        else if (enemyPhase && !gameFinished)
        {
            enemyPhase = false;
            playerPhase = true;
            
            enemyTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            playerTurnStart?.Invoke();
        }
    }

    private void EntityStatsRecovery(Type entityType)
    {
        foreach (Entity entity in entities)
        {
            if (entity.GetType().Equals(entityType))
            {
                // TODO: Find a way to do this in code, not manually listing all stats
                if (!entity.entityStat.currentlyAppliedStatusEffect.HasFlag(StatusEffect.Burn))
                {
                    entity.entityStat.health.IncreaseCurrentValue(entity.entityStat.health.recoveryValue);
                    entity.entityStat.stamina.IncreaseCurrentValue(entity.entityStat.stamina.recoveryValue);
                }
            }
        }
    }

    private async void RunEnemyAI()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.enemyCombat.RunEnemyAI();
            await Task.Delay(2000);
        }

        await Task.Delay(1000);
        TurnEnd();
    }

    private IEnumerator EnemyAttack()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                iterateNextEnemy = false;
                if (enemy.enemyMovement.CheckAttackArea())
                {
                    yield return new WaitForSeconds(1.0f);
                }
                yield return new WaitUntil(() => iterateNextEnemy == true);
            }

            if (gameFinished) break;
        }
        yield return new WaitForSeconds(0.5f);
        TurnEnd();
    }

    public bool OutOfRange(Vector3Int cellgridPosition)
    {
        bool inMoveable = moveableTilemap.HasTile(cellgridPosition);
        bool inObject = objectTilemap.HasTile(cellgridPosition);

        return !inMoveable && !inObject;
    }

    public void SetVirtualCameraFollowTransformTo(Transform follow)
    {
        virtualCameraFollowTransform.SetParent(follow);
        if (follow != null)
        {
            virtualCameraFollowTransform.localPosition = Vector3.zero;
        }
    }

    public Entity EntityExistsAt(Vector3Int cellgridPosition, bool findActive = false, Type entityType = null)
    {
        foreach (Entity entity in entities)
        {
            if (findActive && !entity.gameObject.activeSelf)
            {
                continue;
            }

            if (entityType == null)
            {
                if (entity.entityMovement.currentCellgridPosition.Equals(cellgridPosition))
                {
                    return entity;
                }
            }
            else
            {
                if (entity.entityMovement.currentCellgridPosition.Equals(cellgridPosition) && entity.GetType().Equals(entityType))
                {
                    return entity;
                }
            }
        }

        return null;
    }
}
