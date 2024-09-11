using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Champ_Movement : MonoBehaviour
{
    public static List<GameObject> champions = new List<GameObject>();  // 모든 챔피언을 저장할 리스트
    public bool isSelected = false;
    public Vector3 targetPosition;

    public int moveRange = 4;  // 챔피언의 기본 이동력
    public int currentMoveRange; // 남은 이동력

    private Dictionary<Vector2Int, int> costs = new Dictionary<Vector2Int, int>(); // 타일까지의 이동 비용 저장

    private SpriteRenderer spriteRenderer;  // SpriteRenderer를 이용해 테두리 적용

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();  // SpriteRenderer 초기화

        // 게임 시작 시 각 챔피언을 리스트에 추가 (이 스크립트가 붙어있는 모든 챔피언이 대상)
        if (!champions.Contains(gameObject))
        {
            champions.Add(gameObject);
        }

        currentMoveRange = moveRange; // 초기 이동력을 기본 이동력으로 설정
    }

    private void Update()
    {
        // 우클릭을 감지하여 선택 해제
        if (isSelected && Input.GetMouseButtonDown(1))  // 우클릭 감지
        {
            DeselectChampion();  // 선택 해제
        }
    }

    private void OnMouseDown()
    {
        // 챔피언 선택 상태 변경
        SelectChampion();  // 새로운 챔피언 선택

        // 선택된 챔피언의 이동 가능한 타일들을 계산 및 표시
        ShowMoveableTiles();
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        if (isSelected)
        {
            Vector2Int targetTilePos = FindTileAtPosition(newPosition);  // 목표 타일 찾기

            // 이동할 타일까지의 이동 비용 계산
            if (costs.ContainsKey(targetTilePos))
            {
                int moveCost = costs[targetTilePos];  // Dijkstra 알고리즘에서 계산된 총 이동 비용

                // 남은 이동력보다 많은 비용이 들면 이동하지 않음
                if (currentMoveRange >= moveCost)
                {
                    currentMoveRange -= moveCost;  // 이동 비용 차감
                    targetPosition = newPosition;
                    transform.position = targetPosition;

                    // 이동 후 남은 이동력에 따라 이동 가능한 타일 다시 계산
                    ResetAllTiles();
                    ShowMoveableTiles();
                }
                else
                {
                    Debug.Log("이동할 수 없습니다. 남은 이동력이 부족합니다.");
                }
            }
            else
            {
                Debug.LogError("타일까지의 이동 비용을 찾을 수 없습니다.");
            }
        }
    }

    // 챔피언을 선택하는 함수 (다른 챔피언의 선택 해제 포함)
    public void SelectChampion()
    {
        // 다른 챔피언들의 선택을 해제
        foreach (GameObject champ in champions)
        {
            Champ_Movement champMovement = champ.GetComponent<Champ_Movement>();
            if (champMovement != null && champMovement != this)
            {
                champMovement.DeselectChampion();
            }
        }

        // 현재 챔피언 선택
        isSelected = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }
    }

    // 챔피언 선택 해제 함수
    public void DeselectChampion()
    {
        isSelected = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }

        // 선택 해제 시 타일의 색상 초기화
        ResetAllTiles();
    }

    // 타일의 색상을 초기화하는 함수
    public void ShowMoveableTiles()
    {
        costs.Clear();  // 비용 테이블 초기화
        PriorityQueue<Vector2Int> frontier = new PriorityQueue<Vector2Int>();

        // 현재 Champ의 위치를 기반으로 가장 가까운 타일의 좌표를 찾음
        Vector2Int currentPos = FindTileAtPosition(transform.position);

        if (currentPos == null)
        {
            Debug.LogError("Champ의 위치에 해당하는 타일을 찾을 수 없습니다.");
            return;
        }

        costs[currentPos] = 0;
        frontier.Enqueue(currentPos, 0);

        // Dijkstra 알고리즘 수행
        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentCost = costs[current];

            if (currentCost > moveRange) continue;  // 이동력을 초과한 타일은 무시

            // 현재 타일에서 인접한 타일 검사
            foreach (Vector2Int next in GetNeighbors(current))
            {
                if (Hex_Tile.allTiles.ContainsKey(next))
                {
                    int newCost = currentCost + Hex_Tile.allTiles[next].moveCost;

                    if (!costs.ContainsKey(next) || newCost < costs[next])
                    {
                        costs[next] = newCost;
                        frontier.Enqueue(next, newCost);

                        // 이동 가능한 타일은 회색으로 표시
                        if (newCost <= currentMoveRange)
                        {
                            Hex_Tile.allTiles[next].HighlightTile();
                        }
                    }
                }
            }
        }
    }

    // Champ의 위치에서 가장 가까운 타일을 찾는 함수
    private Vector2Int FindTileAtPosition(Vector3 champPosition)
    {
        // Champ의 실제 위치와 가장 가까운 타일의 좌표를 반환
        float closestDistance = float.MaxValue;
        Vector2Int closestTileCoor = Vector2Int.zero;

        foreach (var tile in Hex_Tile.allTiles)
        {
            float distance = Vector3.Distance(champPosition, tile.Value.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTileCoor = tile.Key;
            }
        }

        return closestTileCoor;
    }

    // 타일들의 색상을 초기화하는 함수
    public void ResetAllTiles()
    {
        foreach (var tile in Hex_Tile.allTiles.Values)
        {
            tile.ResetTileColor();
        }
    }

    // 인접한 타일을 찾는 함수 (육각형 타일 기준)
    private List<Vector2Int> GetNeighbors(Vector2Int tilePos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // 짝수 행에 있는 타일의 인접 타일 좌표
        if (tilePos.x % 2 == 0)
        {
            neighbors.Add(new Vector2Int(tilePos.x - 1, tilePos.y));
            neighbors.Add(new Vector2Int(tilePos.x + 1, tilePos.y));
            neighbors.Add(new Vector2Int(tilePos.x, tilePos.y - 1));
            neighbors.Add(new Vector2Int(tilePos.x, tilePos.y + 1));
            neighbors.Add(new Vector2Int(tilePos.x - 1, tilePos.y + 1));
            neighbors.Add(new Vector2Int(tilePos.x + 1, tilePos.y + 1));
        }
        else
        {
            neighbors.Add(new Vector2Int(tilePos.x - 1, tilePos.y));
            neighbors.Add(new Vector2Int(tilePos.x + 1, tilePos.y));
            neighbors.Add(new Vector2Int(tilePos.x, tilePos.y - 1));
            neighbors.Add(new Vector2Int(tilePos.x, tilePos.y + 1));
            neighbors.Add(new Vector2Int(tilePos.x - 1, tilePos.y - 1));
            neighbors.Add(new Vector2Int(tilePos.x + 1, tilePos.y - 1));
        }
        return neighbors;
    }


/*
    // 특정 조건을 만족했을 때, 챔피언의 리스트를 업데이트하는 함수
    public static void UpdateChampList(GameObject champToRemove, GameObject champToAdd)
    {
        if (champions.Contains(champToRemove))
        {
            champions.Remove(champToRemove);
            Debug.Log(champToRemove.name + "가 리스트에서 제거됨.");
        }
        
        if (!champions.Contains(champToAdd))
        {
            champions.Add(champToAdd);
            Debug.Log(champToAdd.name + "가 리스트에 추가됨.");
        }
    }

    // 특정 조건에 따라 모든 챔피언의 상태를 초기화하는 함수
    public static void ResetAllChampions()
    {
        foreach (GameObject champ in champions)
        {
            Champ_Movement champMovement = champ.GetComponent<Champ_Movement>();
            if (champMovement != null)
            {
                champMovement.isSelected = false;
                Debug.Log(champ.name + "의 선택 상태가 초기화됨.");
            }
        }
    }
*/
}