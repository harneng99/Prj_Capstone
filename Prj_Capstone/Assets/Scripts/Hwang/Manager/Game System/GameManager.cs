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

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Camera mainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
    [field: SerializeField] public Transform virtualCameraFollowTransform { get; private set; }
    [field: SerializeField] public int howManyShouldBeInTheGoal { get; private set; }
    [field: SerializeField] public int turnLimit { get; private set; }
    [field: SerializeField] public bool shouldKillAllEnemies { get; private set; }
    public int howManyCurrentInGoal { get; set; }
    public Entity prevSelectedEntity { get; private set; }
    public Entity currentSelectedEntity { get; private set; }
    public bool pieceDeploymentPhase { get; private set; } = true;
    public bool battlePhase { get; private set; } = false;
    public bool playerPhase { get; private set; } = false;
    public bool didEntityMovedThisTurn { get; set; }
    public bool gamePaused { get; private set; }
    public bool enemyPhase { get; private set; } = false;
    public bool isAiming { get; set; } = false;
    public bool isAimingCopyForFunctionExecutionOrderCorrection { get; set; } = false; // TODO: A better way to implement this
    public int currentTurnCount { get; private set; }
    public Tilemap moveableTilemap { get; private set; }
    public Tilemap objectTilemap { get; private set; }
    public Tilemap selectionTilemap { get; private set; }
    public Tilemap highlightedTilemap { get; private set; }
    public Tilemap fogTilemap { get; private set; }
    public Player mercenaryDragging { get; set; }

    [field: SerializeField] public TileBase selectionTile { get; private set; }
    public Vector3Int currentMouseCellgridPosition { get; private set; }
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Player> playerPieces { get; private set; } = new List<Player>();
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

        playerTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(Player)); };
        playerTurnStart += () => { currentTurnCount += 1; };
        playerTurnStart += () =>
        {
            if (turnLimit < 999)
            {
                Manager.Instance.uiManager.turnCounter.GetComponent<TMP_Text>().text = "Turn " + currentTurnCount + " / " + turnLimit;
            }
            else
            {
                Manager.Instance.uiManager.turnCounter.GetComponent<TMP_Text>().text = "Turn " + currentTurnCount;
            }
        };
        playerTurnStart += () => { Manager.Instance.uiManager.endTurnButton.interactable = true; };
        playerTurnStart += () =>
        { 
            didEntityMovedThisTurn = false;
            foreach (Player playerPiece in playerPieces)
            {
                playerPiece.playerMovement.ResetEntityBooleanVariables();
            }
        };

        playerTurnEnd += Manager.Instance.uiManager.HideInformationUI;
        playerTurnEnd += Manager.Instance.uiManager.ShowPhaseInformationUI;

        enemyTurnStart += () => { if (battlePhase) EntityStatsRecovery(typeof(Enemy)); };
        enemyTurnStart += () => { highlightedTilemap.ClearAllTiles(); };
        enemyTurnStart += () =>
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.enemyMovement.ResetEntityBooleanVariables();
            }
        };

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
        if (!gamePaused)
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
        // Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(entity.transform);
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
        playerPieces = FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        
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
        if (playerPhase && currentTurnCount < turnLimit && shouldKillAllEnemies && !gamePaused)
        {
            bool gameClear = true;

            foreach (Enemy enemy in enemies)
            {
                if (enemy.gameObject.activeSelf || !enemy.isDead)
                {
                    gameClear = false;
                }
            }

            if (gameClear)
            {
                Manager.Instance.uiManager.ShowGameResultWindow("Stage Clear!");
            }
        }

        if (continueTurn) return;

        highlightedTilemap.ClearAllTiles();

        if (playerPhase && currentTurnCount >= turnLimit && !gamePaused)
        {
            Manager.Instance.uiManager.ShowGameResultWindow("Stage Failed...");
        }

        if (playerPhase && !gamePaused)
        {
            playerPhase = false;
            enemyPhase = true;
            
            playerTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            enemyTurnStart?.Invoke();
        }
        else if (enemyPhase && !gamePaused)
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
        if (gamePaused) return;

        continueTurn = false;
        highlightedTilemap.ClearAllTiles();
        Manager.Instance.uiManager.endTurnButton.interactable = false;

        if (playerPhase && !gamePaused)
        {
            playerPhase = false;
            enemyPhase = true;
            
            playerTurnEnd?.Invoke();
            await Task.Delay((int)(Manager.Instance.uiManager.phaseInformationUI.GetComponent<PhaseInformationUI>().uiDuration * 1000));
            enemyTurnStart?.Invoke();
        }
        else if (enemyPhase && !gamePaused)
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
            if (!enemy.isDead)
            {
                iterateNextEnemy = false;

                if (enemy.enemyMovement.CheckAttackArea())
                {
                    yield return new WaitForSeconds(1.0f);
                }
                yield return new WaitUntil(() => iterateNextEnemy == true);
            }

            if (gamePaused) break;
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

    public Entity EntityExistsAt(Vector3Int cellgridPosition, bool onlyFindAlive = false, Type entityType = null, bool onlyFindActive = false)
    {
        if (cellgridPosition == new Vector3Int(-2, -4, 0))
        {
            Debug.Log("Debug Start");
        }

        foreach (Entity entity in entities)
        {
            if (onlyFindAlive && entity.isDead)
            {
                continue;
            }

            if (onlyFindActive && !entity.gameObject.activeSelf)
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

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0.0f;
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1.0f;
    }
}
