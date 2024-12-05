using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatAbilityButton : PooledObject
{
    public Entity entity { get; set; }
    public CombatAbility combatAbility { get; set; }

    public void ToggleCombatAbilityCastRange()
    {
        if (Manager.Instance.gameManager.isAiming)
        {
            Manager.Instance.gameManager.isAiming = false;
            Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = false;
            entity.entityCombat.currentSelectedCombatAbility = null;
            entity.highlightedTilemap.ClearAllTiles();
        }
        else
        {
            Manager.Instance.gameManager.isAiming = true;
            Manager.Instance.gameManager.isAimingCopyForFunctionExecutionOrderCorrection = true;
            entity.entityCombat.currentSelectedCombatAbility = combatAbility;
            entity.entityCombat.DrawCastingRange(combatAbility);
        }
        
    }
}
