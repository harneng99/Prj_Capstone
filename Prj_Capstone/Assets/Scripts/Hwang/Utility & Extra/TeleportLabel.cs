using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Label { One, Two, Three, Four, Five }

public class TeleportLabel : MonoBehaviour
{
    [field: SerializeField] public Label teleportLabel { get; private set; }
    public Vector3Int cellgridPosition { get; private set; }
    private Tilemap interactableTilemap;

    private void Awake()
    {
        interactableTilemap = GameObject.FindWithTag("InteractableTilemap").GetComponent<Tilemap>();
        cellgridPosition = interactableTilemap.WorldToCell(transform.position);
    }

    private void Start()
    {
        GameObject cellgridObject = interactableTilemap.GetInstantiatedObject(cellgridPosition);
        CustomTileData customTileData = cellgridObject.GetComponent<CustomTileData>();
        Animator[] animator = cellgridObject.GetComponentsInChildren<Animator>();

        animator[0].SetInteger("Label", (int)teleportLabel);
        if (customTileData != null && customTileData.tileLayerType == TileType.Interactable && customTileData.interactableTileLayer == InteractableTileLayer.UnidirectionalTeleport)
        {
            animator[1].SetBool("IsEntrance", customTileData.entrance);
        }
    }
}
