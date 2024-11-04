using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Label { One, Two, Three, Four, Five }

public class TeleportLabel : MonoBehaviour
{
    [field: SerializeField] public Label teleportLabel { get; private set; }
    public Vector3Int cellgridPosition { get; private set; }

    private void Awake()
    {
        cellgridPosition = GameObject.FindWithTag("InteractableTilemap").GetComponent<Tilemap>().WorldToCell(transform.position);
    }
}
