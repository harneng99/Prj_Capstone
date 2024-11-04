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
        Vector3 horizontalOffset = new Vector3(cellSize.x / 4, 0.0f, 0.0f);
        Vector3 verticalOffset = new Vector3(0.0f, cellSize.y / 8, 0.0f);

        /*for (int x = -hexgridXWidth; x <= hexgridXWidth; x++)
        {
            for (int y = -hexgridYHeight; y <= hexgridYHeight; y++)
            {
                for (int z = -hexgridZWidth; z <= hexgridZWidth; z++)
                {
                    if (x + y + z != 0) continue;

                    // Draw Hexgrid
                    Vector3Int currentHexgridPosition = new Vector3Int(x, y, z);
                    Vector3 currentWorldgridPosition = HexgridToWorldgrid(currentHexgridPosition);
                    Vector3Int currentCellgridPosition = HexgridToCellgrid(currentHexgridPosition);
                    Vector3 worldgridTopPosition = currentWorldgridPosition + new Vector3(0.0f, cellSize.y / 2);
                    Vector3 worldgridTopLeftPosition = currentWorldgridPosition - new Vector3(cellSize.x / 2, -cellSize.y / 4);
                    Vector3 worldgridTopRightPosition = currentWorldgridPosition + new Vector3(cellSize.x / 2, cellSize.y / 4);
                    Vector3 worldgridBottomPosition = currentWorldgridPosition - new Vector3(0.0f, cellSize.y / 2);
                    Vector3 worldgridBottomLeftPosition = currentWorldgridPosition - new Vector3(cellSize.x / 2, cellSize.y / 4);
                    Vector3 worldgridBottomRightPosition = currentWorldgridPosition + new Vector3(cellSize.x / 2, -cellSize.y / 4);

                    Handles.Label(currentWorldgridPosition - horizontalOffset + verticalOffset, currentHexgridPosition.ToString());
                    Handles.Label(currentWorldgridPosition - horizontalOffset - verticalOffset, new Vector2Int(currentCellgridPosition.x, currentCellgridPosition.y).ToString());

                    Gizmos.DrawLine(worldgridTopPosition, worldgridTopRightPosition);
                    Gizmos.DrawLine(worldgridTopRightPosition, worldgridBottomRightPosition);
                    Gizmos.DrawLine(worldgridBottomRightPosition, worldgridBottomPosition);
                    Gizmos.DrawLine(worldgridBottomPosition, worldgridBottomLeftPosition);
                    Gizmos.DrawLine(worldgridBottomLeftPosition, worldgridTopLeftPosition);
                    Gizmos.DrawLine(worldgridTopLeftPosition, worldgridTopPosition);
                }
            }
        }*/

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

            Handles.Label(worldgridTopLeftPosition - horizontalOffset - verticalOffset, new Vector2Int(cellgridPosition.x, cellgridPosition.y).ToString());
        }
    }
}
