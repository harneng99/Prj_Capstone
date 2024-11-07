using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

public enum TargetType { Closest, LowestHealth, PotentialThreat }

public class WhatToDo
{
    // TODO: Add logic about potential threat type mercenary
    public float score { get; private set; }
    public CombatAbility combatAbility { get; private set; }
    public Vector3Int enemyHexgridPosition { get; private set; }
    public Vector3Int combatAbilityHexgridCenter { get; private set; }
    public bool moveAfterAttack { get; private set; }

    public WhatToDo(float score, CombatAbility combatAbility, Vector3Int enemyHexgridPosition, Vector3Int combatAbilityHexgridCenter, bool moveAfterAttack)
    {
        this.score = score;
        this.combatAbility = combatAbility;
        this.enemyHexgridPosition = enemyHexgridPosition;
        this.combatAbilityHexgridCenter = combatAbilityHexgridCenter;
        this.moveAfterAttack = moveAfterAttack;
    }
}

public class EnemyCombat : Combat
{
    public PlayerCharacter currentTarget { get; private set; }

    [Header("AI Target")]
    [SerializeField, Tooltip("List components should not be duplicated. Score multiplier value decreases as the priority is low, by scoreMultiplierDecreaseAmount * priority.")] private List<TargetType> targetPriority;
    [SerializeField] private float scoreMultiplierDecreaseAmount = 0.2f;

    [field: Header("AI Score Per")]
    [field: SerializeField] public float mercenaryKill { get; private set; }
    [field: SerializeField] public float mercenaryDamage { get; private set; }
    [SerializeField] private float distanceFromClosestMercenary;
    [SerializeField] private float meanDistanceFromMercenaries;
    [SerializeField] private float remainingStamina;
    [SerializeField] private bool changeByCurrentHealth;

    private Enemy enemy;

    protected override void Awake()
    {
        base.Awake();

        enemy = entity as Enemy;
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (Manager.Instance.gameManager.battlePhase)
            {
                if (!Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection)
                {
                    Manager.Instance.uiManager.SetCombatAbilityButtons();
                }
            }
        }
    }

    public override void MouseRightClick()
    {
        if (Manager.Instance.gameManager.battlePhase)
        {
            if (entity.isSelected)
            {
                currentSelectedCombatAbility = null;
                Manager.Instance.gameManager.isAiming = false;
                Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
                entity.highlightedTilemap.ClearAllTiles();
                aoeTilemap.ClearAllTiles();
            }
        }
    }

    public async void RunEnemyAI()
    {
        // TODO: Change camera position
        float currentMaxScore = float.MinValue;
        WhatToDo highestScoreWhatToDo = null;

        for (int i = 0; i < targetPriority.Count; i++)
        {
            PlayerCharacter target = SetTarget(targetPriority[i]);

            if (target == null) continue;

            WhatToDo whatToDo = DecideWhatToDo(target);

            if (currentMaxScore < whatToDo.score * (1.0f - scoreMultiplierDecreaseAmount * i))
            {
                highestScoreWhatToDo = whatToDo;
                currentMaxScore = whatToDo.score * (1.0f - scoreMultiplierDecreaseAmount * i);
            }
        }

        if (highestScoreWhatToDo == null)
        {
            Debug.LogWarning(enemy.name + " cannot decide what to do.");
            return;
        }

        Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(enemy.transform);

        if (highestScoreWhatToDo.moveAfterAttack)
        {
            if (highestScoreWhatToDo.combatAbility != null)
            {
                await Task.Delay(200);
                enemy.enemyCombat.DrawCastingRange(highestScoreWhatToDo.combatAbility);

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
                Manager.Instance.gameManager.virtualCameraFollowTransform.position = enemy.entityMovement.pathfinder.HexgridToWorldgrid(highestScoreWhatToDo.combatAbilityHexgridCenter);
                enemy.enemyCombat.DrawAOE(enemy.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.combatAbilityHexgridCenter), highestScoreWhatToDo.combatAbility);

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(enemy.transform);
                enemy.highlightedTilemap.ClearAllTiles();
                aoeTilemap.ClearAllTiles();
                ExecuteCombatAbility(highestScoreWhatToDo.combatAbilityHexgridCenter, GridType.Hexgrid, highestScoreWhatToDo.combatAbility);
            }

            if (!highestScoreWhatToDo.enemyHexgridPosition.Equals(enemy.entityMovement.currentHexgridPosition))
            {
                await Task.Delay(200);
                enemy.entityMovement.DrawMoveableTilemap();

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
                Manager.Instance.gameManager.virtualCameraFollowTransform.position = enemy.entityMovement.pathfinder.HexgridToWorldgrid(highestScoreWhatToDo.enemyHexgridPosition);
                Manager.Instance.gameManager.selectionTilemap.SetTile(entity.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.enemyHexgridPosition), Manager.Instance.gameManager.selectionTile);

                await Task.Delay(200);
                enemy.highlightedTilemap.ClearAllTiles();
                Manager.Instance.gameManager.selectionTilemap.SetTile(entity.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.enemyHexgridPosition), null);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(enemy.transform);
                enemy.entityMovement.MoveToGrid(highestScoreWhatToDo.enemyHexgridPosition, GridType.Hexgrid, false);
                if (enemy.entityMovement.isMoving)
                {
                    await Task.Delay(100);
                }
            }
        }
        else
        {
            if (!highestScoreWhatToDo.enemyHexgridPosition.Equals(enemy.entityMovement.currentHexgridPosition))
            {
                await Task.Delay(200);
                enemy.entityMovement.DrawMoveableTilemap();

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
                Manager.Instance.gameManager.virtualCameraFollowTransform.position = enemy.entityMovement.pathfinder.HexgridToWorldgrid(highestScoreWhatToDo.enemyHexgridPosition);
                Manager.Instance.gameManager.selectionTilemap.SetTile(entity.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.enemyHexgridPosition), Manager.Instance.gameManager.selectionTile);

                await Task.Delay(200);
                enemy.highlightedTilemap.ClearAllTiles();
                Manager.Instance.gameManager.selectionTilemap.SetTile(entity.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.enemyHexgridPosition), null);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(enemy.transform);
                enemy.entityMovement.MoveToGrid(highestScoreWhatToDo.enemyHexgridPosition, GridType.Hexgrid, false);
                while (enemy.entityMovement.isMoving)
                {
                    await Task.Delay(100);
                }
            }

            if (highestScoreWhatToDo.combatAbility != null)
            {
                await Task.Delay(200);
                enemy.enemyCombat.DrawCastingRange(highestScoreWhatToDo.combatAbility);

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(null);
                Manager.Instance.gameManager.virtualCameraFollowTransform.position = enemy.entityMovement.pathfinder.HexgridToWorldgrid(highestScoreWhatToDo.combatAbilityHexgridCenter);
                enemy.enemyCombat.DrawAOE(enemy.entityMovement.pathfinder.HexgridToCellgrid(highestScoreWhatToDo.combatAbilityHexgridCenter), highestScoreWhatToDo.combatAbility);

                await Task.Delay(200);
                Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(enemy.transform);
                enemy.highlightedTilemap.ClearAllTiles();
                aoeTilemap.ClearAllTiles();
                ExecuteCombatAbility(highestScoreWhatToDo.combatAbilityHexgridCenter, GridType.Hexgrid, highestScoreWhatToDo.combatAbility);
            }
        }

        await Task.Delay(500); // TODO: Wait until animation finishes
    }

    public PlayerCharacter SetTarget(TargetType targetType)
    {
        switch (targetType)
        {
            case TargetType.Closest:
                return ClosestDistance();

            case TargetType.LowestHealth:
                return LowestHealth();

            case TargetType.PotentialThreat:
                return null;

            default:
                return null;
        }
    }

    private PlayerCharacter ClosestDistance()
    {
        PlayerCharacter closestDistanceTarget = null;
        int closestDistance = int.MaxValue;
        int closestDistanceStaminaCost = int.MaxValue;
        PlayerCharacter lowestStaminaTarget = null;
        int lowestStaminaCost = int.MaxValue;

        foreach (PlayerCharacter mercenary in Manager.Instance.gameManager.mercenaries)
        {
            if (!mercenary.isActiveAndEnabled) continue;

            if (entity.entityMovement.pathfinder.GetHeuristicDistance(entity.entityMovement.currentHexgridPosition, mercenary.entityMovement.currentHexgridPosition) > closestDistance) continue;

            PathInformation pathInformation = entity.entityMovement.pathfinder.PathFinding(entity.entityMovement.currentCellgridPosition, mercenary.entityMovement.currentCellgridPosition);
            
            if (pathInformation != null)
            {
                if (closestDistance > pathInformation.path.Count)
                {
                    closestDistanceTarget = mercenary;
                    closestDistance = pathInformation.path.Count;
                    closestDistanceStaminaCost = pathInformation.requiredStamina;
                }

                if (lowestStaminaCost > pathInformation.requiredStamina)
                {
                    lowestStaminaTarget = mercenary;
                    lowestStaminaCost = pathInformation.requiredStamina;
                }
            }
        }

        if (closestDistanceTarget != null)
        {
            return closestDistanceTarget;
        }
        else if (lowestStaminaTarget != null)
        {
            return lowestStaminaTarget;
        }
        else return null;
    }

    // TODO: If there are many mercenaries with the same health, then sort through distance or potential threat.
    private PlayerCharacter LowestHealth()
    {
        PlayerCharacter currentLowestHealthTarget = null;
        float lowestHealth = float.MaxValue;

        foreach (PlayerCharacter mercenary in Manager.Instance.gameManager.mercenaries)
        {
            if (lowestHealth > mercenary.entityStat.health.currentValue)
            {
                currentLowestHealthTarget = mercenary;
                lowestHealth = mercenary.entityStat.health.currentValue;
            }
            /*else if (lowestHealth == mercenary.entityStat.health.currentValue)
            {
                PathInformation originalMiminumHealthPathInformation = entity.entityMovement.pathfinder.PathFinding(entity.entityMovement.currentCellgridPosition, currentLowestHealthTarget.entityMovement.currentCellgridPosition);
                PathInformation newMinimumHealthPathInformation = entity.entityMovement.pathfinder.PathFinding(entity.entityMovement.currentCellgridPosition, mercenary.entityMovement.currentCellgridPosition);
            }*/
        }

        return currentLowestHealthTarget;
    }

    private WhatToDo DecideWhatToDo(PlayerCharacter targetMercenary)
    {
        Vector3Int targetCellgridPosition = targetMercenary.entityMovement.currentCellgridPosition;
        Vector3Int targetHexgridPosition = targetMercenary.entityMovement.currentHexgridPosition;

        float highestScore = int.MinValue;
        CombatAbility highestScoreCombatAbility = null;
        Vector3Int highestScoreEnemyHexgridPosition = Vector3Int.one;
        Vector3Int highestScoreCombatAbilityHexgridCenter = Vector3Int.one;

        foreach (CombatAbility combatAbility in combatAbilities) // for all combat abilities
        {
            if (combatAbility.staminaCost > entity.entityStat.stamina.currentValue) continue; // if the remaining stamina is not enough, skip the combat ability

            foreach (Vector3Int hexgridAOEOffset in combatAbility.AOEDictionary.Keys) // for all area of effect of the chosen combat ability
            {
                Vector3Int combatAbilityHexgridCenterPosition = targetHexgridPosition - hexgridAOEOffset; // decide where the enemy will cast its combat ability

                foreach (Vector3Int hexgridCastingRangeOffset in combatAbility.castingRangeDictionary.Keys) // for all range of casting of the chosen combat ability
                {
                    Vector3Int enemyHexgridPosition = combatAbilityHexgridCenterPosition - hexgridCastingRangeOffset; // decide where the enemy will go to cast its combat ability
                    Vector3Int enemyCellgridPosition = enemy.entityMovement.pathfinder.HexgridToCellgrid(enemyHexgridPosition);

                    if (entity.entityMovement.pathfinder.GetHeuristicDistance(entity.entityMovement.currentHexgridPosition, enemyHexgridPosition) > entity.entityStat.stamina.currentValue) continue; // if heuristic distance is bigger than left over stamina, then the enemy can't use its combat ability, so skip

                    PathInformation pathInformation = entity.entityMovement.pathfinder.PathFinding(entity.entityMovement.currentCellgridPosition, enemyCellgridPosition, false);

                    if (pathInformation == null || pathInformation.requiredStamina + combatAbility.staminaCost > entity.entityStat.stamina.currentValue) continue; // if the stamina cost of the movement and the chosen combat ability is bigger than remaining stamina, then skip

                    if (enemy.entityMovement.PathOutOfRange(enemy.entityMovement.currentHexgridPosition, pathInformation)) continue;

                    float currentScore = CalculateScore(enemyHexgridPosition, combatAbilityHexgridCenterPosition, combatAbility); // calculate score for movement

                    if (currentScore > highestScore)
                    {
                        highestScore = currentScore;
                        highestScoreCombatAbility = combatAbility;
                        highestScoreEnemyHexgridPosition = enemyHexgridPosition;
                        highestScoreCombatAbilityHexgridCenter = combatAbilityHexgridCenterPosition;
                    }
                }
            }
        }

        bool moveAfterAttack = false;

        if (highestScoreEnemyHexgridPosition.Equals(entity.entityMovement.currentHexgridPosition))
        {
            float highestDistanceScore = DistanceScore(highestScoreEnemyHexgridPosition);
            moveAfterAttack = true;

            for (int x = -entity.entityMovement.moveRangeInHexGrid.x; x <= entity.entityMovement.moveRangeInHexGrid.x; x++)
            {
                for (int y = -entity.entityMovement.moveRangeInHexGrid.y; y <= entity.entityMovement.moveRangeInHexGrid.y; y++)
                {
                    for (int z = -entity.entityMovement.moveRangeInHexGrid.z; z <= entity.entityMovement.moveRangeInHexGrid.z; z++)
                    {
                        if (x + y + z != 0) continue;

                        Vector3Int moveableHexgridPosition = entity.entityMovement.currentHexgridPosition + new Vector3Int(x, y, z);
                        Vector3Int moveableCellgridPosition = entity.entityMovement.pathfinder.HexgridToCellgrid(moveableHexgridPosition);

                        PathInformation pathInformation = entity.entityMovement.pathfinder.PathFinding(entity.entityMovement.currentCellgridPosition, moveableCellgridPosition);

                        if (pathInformation == null || entity.entityStat.stamina.currentValue < pathInformation.requiredStamina + highestScoreCombatAbility.staminaCost) continue;

                        float currentDistanceScore = DistanceScore(moveableHexgridPosition);
                        
                        if (currentDistanceScore > highestDistanceScore)
                        {
                            highestDistanceScore = currentDistanceScore;
                            highestScoreEnemyHexgridPosition = moveableHexgridPosition;
                        }
                    }
                }
            }
        }

        return new WhatToDo(highestScore, highestScoreCombatAbility, highestScoreEnemyHexgridPosition, highestScoreCombatAbilityHexgridCenter, moveAfterAttack);
    }

    private float CalculateScore(Vector3Int enemyHexgridPosition, Vector3Int combatAbilityHexgridCenterPosition, CombatAbility combatAbility)
    {
        float combatAbilityScore = CombatAbilityScore(combatAbilityHexgridCenterPosition, combatAbility);
        float distanceScore = DistanceScore(enemyHexgridPosition);
        float remainingStaminaScore = (entity.entityStat.stamina.currentValue - combatAbility.staminaCost) * remainingStamina;

        return combatAbilityScore + distanceScore + remainingStaminaScore;
    }

    private float DistanceScore(Vector3Int enemyHexgridPosition)
    {
        Vector3Int enemyCellgridPosition = enemy.entityMovement.pathfinder.HexgridToCellgrid(enemyHexgridPosition);
        int activeMercenariesCount = 0;
        int minimumStaminaCostFromMercenary = int.MaxValue;
        float meanStaminaCostFromMercenaries = 0;

        foreach (PlayerCharacter mercenary in Manager.Instance.gameManager.mercenaries)
        {
            if (!mercenary.isActiveAndEnabled) continue;

            activeMercenariesCount += 1;
            int staminaCostFromMercenary = entity.entityMovement.pathfinder.PathFinding(mercenary.entityMovement.currentCellgridPosition, enemyCellgridPosition).requiredStamina;

            meanStaminaCostFromMercenaries += staminaCostFromMercenary;
            minimumStaminaCostFromMercenary = Mathf.Min(minimumStaminaCostFromMercenary, staminaCostFromMercenary);
        }
        meanStaminaCostFromMercenaries /= activeMercenariesCount;

        return minimumStaminaCostFromMercenary * distanceFromClosestMercenary + meanStaminaCostFromMercenaries * meanDistanceFromMercenaries;
    }

    private float CombatAbilityScore(Vector3Int combatAbilityHexgridCenterPosition, CombatAbility combatAbility)
    {
        float score = 0;

        foreach (Vector3Int rangeHexgridOffset in combatAbility.AOEDictionary.Keys)
        {
            if (combatAbility.AOEDictionary[rangeHexgridOffset] == false) continue;

            Vector3Int currentRangeHexgrid = combatAbilityHexgridCenterPosition + rangeHexgridOffset;

            foreach (Entity entity in Manager.Instance.gameManager.entities)
            {
                if (!entity.isActiveAndEnabled) continue;

                if (entity.entityMovement.currentHexgridPosition.Equals(currentRangeHexgrid))
                {
                    foreach (CombatAbilityComponent combatAbilityComponent in combatAbility.combatAbilityComponents)
                    {
                        score += combatAbilityComponent.GetEnemyAIScore(entity);
                    }
                }
            }
        }

        return score;
    }
}
