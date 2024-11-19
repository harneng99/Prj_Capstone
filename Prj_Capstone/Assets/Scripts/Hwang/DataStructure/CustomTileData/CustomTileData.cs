using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Moveable, Object, Interactable, Highlight }
[System.Flags] public enum MoveableTileLayer { Ground = 1 << 0, Water = 1 << 1 }
public enum ObjectTileLayer { Swamp = 1 << 0, Wall = 1 << 1 }
public enum InteractableTileLayer { Goal, UnidirectionalTeleport, BidirectionalTeleport, Promotion }

public class CustomTileData : MonoBehaviour
{
    [field: SerializeField] public TileType tileLayerType { get; private set; }
    [field: SerializeField] public int tileLevel { get; private set; }
    [field: SerializeField] public MoveableTileLayer moveableTileLayer { get; private set; }
    [field: SerializeField] public ObjectTileLayer objectTileLayer { get; private set; }
    [field: SerializeField] public InteractableTileLayer interactableTileLayer { get; private set; }
    [field: SerializeField] public bool entrance { get; private set; }
    [field: SerializeField] public string tileInformation { get; private set; }
    public Animator animator { get; private set; }
    
    [SerializeField] private TileBase destroyedPillarTileBase;
    [SerializeField] private TileBase destroyedSpikeTileBase;
    private Tilemap objectTilemap;
    private Tilemap foregroundDecorationTilemap;
    private Vector3Int cellgridPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        objectTilemap = GameObject.FindWithTag("ObjectTilemap").GetComponent<Tilemap>();
        foregroundDecorationTilemap = GameObject.FindWithTag("ForegroundDecorationTilemap").GetComponent<Tilemap>();
    }

    public void ChangeToMoveableTile(Vector3Int cellgridPosition)
    {
        this.cellgridPosition = cellgridPosition;
        tileLayerType = TileType.Moveable;
        objectTilemap.SetTile(cellgridPosition, null);
        if (objectTileLayer == ObjectTileLayer.Wall)
        {
            foregroundDecorationTilemap.SetTile(cellgridPosition + Vector3Int.up, null);
        }

        animator.SetTrigger("Destroy");
    }

    public void ChangeTileBase()
    {
        if (objectTileLayer == ObjectTileLayer.Swamp)
        {
            objectTilemap.SetTile(cellgridPosition, destroyedSpikeTileBase);
        }
        else if (objectTileLayer == ObjectTileLayer.Wall)
        {
            objectTilemap.SetTile(cellgridPosition, destroyedPillarTileBase);
        }
    }
}
