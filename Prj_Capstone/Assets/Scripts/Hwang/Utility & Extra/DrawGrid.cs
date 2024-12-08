using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawGrid : MonoBehaviour
{
    [SerializeField] Grid gridBase;
    [SerializeField] private Tilemap moveableTilemap;

    private void OnDrawGizmos()
    {
        Vector3 cellSize = gridBase.cellSize;
        Gizmos.color = Color.green;
        Vector3 horizontalOffset = new Vector3(cellSize.x / 8, 0.0f, 0.0f);
        Vector3 verticalOffset = new Vector3(0.0f, cellSize.y / 2, 0.0f);

        foreach (Vector3Int cellgridPosition in moveableTilemap.cellBounds.allPositionsWithin)
        {
            Vector3 worldgridBottomLeftPosition = moveableTilemap.CellToWorld(cellgridPosition);
            Vector3 worldgridBottomRightPosition = worldgridBottomLeftPosition + Vector3.right;
            Vector3 worldgridTopLeftPosition = worldgridBottomLeftPosition + Vector3.up;
            Vector3 worldgridTopRightPosition = worldgridBottomLeftPosition + Vector3.right + Vector3.up;

            Gizmos.DrawLine(worldgridTopLeftPosition, worldgridBottomLeftPosition);
            Gizmos.DrawLine(worldgridBottomLeftPosition, worldgridBottomRightPosition);
            Gizmos.DrawLine(worldgridBottomRightPosition, worldgridTopRightPosition);
            Gizmos.DrawLine(worldgridTopRightPosition, worldgridTopLeftPosition);

            Handles.Label(worldgridTopLeftPosition + horizontalOffset - verticalOffset, new Vector2Int(cellgridPosition.x, cellgridPosition.y).ToString());
        }
    }
}
