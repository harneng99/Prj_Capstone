using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex_Tile : MonoBehaviour
{
    public int moveCost = 1; // 각 타일의 이동 비용 (기본값 1)
    public bool IsHighlighted = false; // 타일이 하이라이트되어 있는지 여부
    public static Dictionary<Vector2Int, Hex_Tile> allTiles = new Dictionary<Vector2Int, Hex_Tile>(); // 모든 타일 저장
    public Vector2Int coor;  // 타일의 좌표
    private Color originalColor; // 타일의 기본 색상 저장 변수

    private void Start()
    {
        // 이름을 파싱하여 Vector2Int 좌표로 변환
        coor = ParseTileName(gameObject.name);
        
        // 타일을 좌표로 저장
        if (!allTiles.ContainsKey(coor))
        {
            allTiles.Add(coor, this);
        }

        // 기본 색상 저장
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private Vector2Int ParseTileName(string tileName)
    {
        // 타일의 이름을 "(x, y)" 형식으로 가정하고 파싱
        tileName = tileName.Trim(new char[] { 'T', 'i', 'l', 'e', '(', ')' });
        string[] coords = tileName.Split(',');
        int x = int.Parse(coords[0]);
        int y = int.Parse(coords[1]);
        return new Vector2Int(x, y);
    }

    private void OnMouseDown()
    {
        // IsHighlighted가 true일 때만 타일이 반응하도록 설정
        if (IsHighlighted)
        {
            // 모든 챔피언 리스트를 확인하여, 선택된 챔피언만 이동시킴
            foreach (GameObject champ in Champ_Movement.champions)
            {
                Champ_Movement champ_movement = champ.GetComponent<Champ_Movement>();
                
                if (champ_movement != null && champ_movement.isSelected)  // 선택된 챔피언만 이동
                {
                    Vector3 targetPosition = transform.position;  // 타일의 위치로 이동
                    champ_movement.MoveToPosition(targetPosition); // 챔피언 이동
                }
            }
        }
    }

    // 타일 색상을 회색으로 변경하고 하이라이트 상태로 설정하는 함수
    public void HighlightTile()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 현재 색상 값 가져오기 (r, g, b는 0~1 범위)
            Color currentColor = spriteRenderer.color;

            // RGB 값을 100씩 감소시키기 위한 변환 (255 기준으로 연산 후 다시 0~1로 변환)
            float newR = Mathf.Max(0, currentColor.r * 255 - 50) / 255f;
            float newG = Mathf.Max(0, currentColor.g * 255 - 50) / 255f;
            float newB = Mathf.Max(0, currentColor.b * 255 - 50) / 255f;

            // 변경된 색상 적용
            spriteRenderer.color = new Color(newR, newG, newB, currentColor.a);
        }
        IsHighlighted = true;  // 하이라이트 상태로 변경
    }

    // 타일 색상을 기본 색으로 되돌리고 하이라이트 상태를 해제하는 함수
    public void ResetTileColor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 기본 색상으로 되돌림
            spriteRenderer.color = originalColor;
        }
        IsHighlighted = false;  // 하이라이트 해제
    }
}