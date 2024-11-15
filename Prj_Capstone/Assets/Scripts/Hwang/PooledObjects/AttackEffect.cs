using Dev.ComradeVanti.WaitForAnim;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AttackEffect : MonoBehaviour
{
    [SerializeField] private TileBase pillarUpperPartTileBase;

    private Tilemap objectTilemap;
    private Tilemap foregroundDecorationTilemap;
    private Entity entity;
    private Vector3Int targetCellgridPosition;

    private void Awake()
    {
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();
        foregroundDecorationTilemap = GameObject.FindWithTag("ForegroundDecorationTilemap").GetComponent<Tilemap>();
    }

    public void SetTargetPosition(Entity entity, Vector3Int targetCellgridPosition)
    {
        this.entity = entity;
        this.targetCellgridPosition = targetCellgridPosition;
    }

    public IEnumerator Attack()
    {
        Entity targetEntity = null;

        if (entity == null) yield break;

        if (entity.GetType().Equals(typeof(PlayerCharacter)))
        {
            targetEntity = Manager.Instance.gameManager.EntityExistsAt(targetCellgridPosition, true, typeof(Enemy));
        }
        else if (entity.GetType().Equals(typeof(Enemy)))
        {
            targetEntity = Manager.Instance.gameManager.EntityExistsAt(targetCellgridPosition, true, typeof(PlayerCharacter));
        }

        targetEntity.animator.SetTrigger("Hurt");

        if (entity.GetType().Equals(typeof(PlayerCharacter)))
        {
            objectTilemap.SetTile(targetCellgridPosition, null);
            if (foregroundDecorationTilemap.GetTile(targetCellgridPosition + Vector3Int.up) == pillarUpperPartTileBase)
            {
                foregroundDecorationTilemap.SetTile(targetCellgridPosition + Vector3Int.up, null);
            }
            Destroy(objectTilemap.GetInstantiatedObject(targetCellgridPosition));

            yield return new WaitForAnimationToFinish(targetEntity.animator, "Hurt");
            yield return new WaitForAnimationToFinish(targetEntity.animator, "Death");

            Manager.Instance.gameManager.continueTurn = false;
            Manager.Instance.gameManager.TurnEnd();
        }
    }
}
