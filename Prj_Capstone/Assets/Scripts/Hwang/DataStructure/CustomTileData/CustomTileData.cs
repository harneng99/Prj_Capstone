using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Moveable, Object, Interactable, Highlight }
[System.Flags] public enum MoveableTileLayer { Ground = 1 << 0, Water = 1 << 1 }
[System.Flags] public enum ObjectTileLayer { Mountain = 1 << 0, Wall = 1 << 1 }
[System.Flags] public enum InteractableTileLayer { CurrentlyNothing = 1 << 0 }

public class CustomTileData : MonoBehaviour
{
    [field: SerializeField] public TileType tileLayerType { get; private set; }
    [field: SerializeField] public int tileLevel { get; private set; }
    [field: SerializeField] public MoveableTileLayer moveableTileLayer { get; private set; }
    [field: SerializeField] public ObjectTileLayer objectTileLayer { get; private set; }
    [field: SerializeField] public InteractableTileLayer interactableTileLayer { get; private set; }
    [field: SerializeField] public string tileInformation { get; private set; }
}
