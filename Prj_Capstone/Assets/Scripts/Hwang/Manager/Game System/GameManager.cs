using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public int currentTurnCount { get; private set; } = 1;
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
        playerTurnStart += () => { Manager.Instance.uiManager.endTurnButton.interactable = true; };
        playerTurnStart += () =>
        { 
            didEntityMovedThisTurn = false;
            foreach (Player playerPiece in playerPieces)
            {
                playerPiece.playerMovement.ResetEntityBooleanVariables();
            }
        };

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

        enemyTurnEnd += () => { currentTurnCount += 1; };
        enemyTurnEnd += () =>
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
        enemyTurnEnd += Manager.Instance.uiManager.ShowPhaseInformationUI;
    }

    private void Start()
    {
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
        Time.timeScale = 1.0f;
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        entities = FindObjectsByType<Entity>(FindObjectsSortMode.None).ToList();
        playerPieces = FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();

        Manager.Instance.uiManager.turnCounter.SetActive(true);
        if (shouldKillAllEnemies)
        {
            Manager.Instance.uiManager.enemyCounter.SetActive(true);
        }
        else
        {
            Manager.Instance.uiManager.SetGoalCounter(true, "Goal " + howManyCurrentInGoal + " / " + howManyShouldBeInTheGoal);
        }

        playerTurnStart?.Invoke();
        if (turnLimit < 999)
        {
            Manager.Instance.uiManager.turnCounter.GetComponent<TMP_Text>().text = "Turn " + currentTurnCount + " / " + turnLimit;
        }
        else
        {
            Manager.Instance.uiManager.turnCounter.GetComponent<TMP_Text>().text = "Turn " + currentTurnCount;
        }
        ResetEntitySelected();
        pieceDeploymentPhase = false;
        battlePhase = true;
        playerPhase = true;
        enemyPhase = false;
        Manager.Instance.uiManager.endTurnButton.interactable = false;
        Manager.Instance.uiManager.ShowPhaseInformationUI();
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
                PlayerPrefs.SetInt("stageclear", 1);
                Manager.Instance.uiManager.ShowGameResultWindow("Stage Clear!");
                return;
            }
        }

        if (continueTurn) return;

        highlightedTilemap.ClearAllTiles();

        if (playerPhase && currentTurnCount >= turnLimit && !gamePaused)
        {
            PlayerPrefs.SetInt("stageclear", 0);
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetResolution(int setWidth, int setHeight)
    {
        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight)
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight);
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight);
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
    }
}