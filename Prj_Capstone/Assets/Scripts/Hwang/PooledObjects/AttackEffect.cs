using Dev.ComradeVanti.WaitForAnim;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AttackEffect : PooledObject
{
    [SerializeField] private TileBase pillarUpperPartTileBase;

    private Tilemap objectTilemap;
    private Entity entity;
    private Vector3Int targetCellgridPosition;

    private void Awake()
    {
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();
    }

    public void SetAttackEffectTarget(Entity entity, Vector3Int targetCellgridPosition)
    {
        this.entity = entity;
        this.targetCellgridPosition = targetCellgridPosition;
        transform.position = targetCellgridPosition + new Vector3(0.5f, 0.5f, 0.0f);
        if (gameObject.name.Contains("EnemyQueenAttackEffect1"))
        {
            transform.position += new Vector3(-0.1f, 1.0f, 0.0f);
        }
    }

    public void Attack(int killTargetEntity = 0)
    {
        if (entity == null)
        {
            Manager.Instance.gameManager.continueTurn = false;
            Manager.Instance.gameManager.TurnEnd();
            return;
        }

        if (entity.GetType().Equals(typeof(Player)))
        {
            entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(targetCellgridPosition, false, typeof(Enemy));
        }
        else if (entity.GetType().Equals(typeof(Enemy)))
        {
            entity.entityCombat.targetEntity = Manager.Instance.gameManager.EntityExistsAt(targetCellgridPosition, false, typeof(Player));
        }

        entity.AttackEntity(killTargetEntity);

        if (entity.GetType().Equals(typeof(Player)))
        {
            CustomTileData customTileData = objectTilemap.GetInstantiatedObject(targetCellgridPosition)?.GetComponent<CustomTileData>();
            customTileData?.ChangeToMoveableTile(targetCellgridPosition);
        }
    }
}
