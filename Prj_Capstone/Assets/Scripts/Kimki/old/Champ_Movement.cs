using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Champ_Movement : MonoBehaviour
{
    public static List<GameObject> champions = new List<GameObject>();
    public bool isSelected = false;
    public Vector3 targetPosition;
    public int moveRange = 4;
    public int currentMoveRange;

    private Dictionary<Vector2Int, int> costs = new Dictionary<Vector2Int, int>(); // 이동 비용
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // 경로 추적
    private SpriteRenderer spriteRenderer;

    // 턴 관리
    public static bool isPlayerTurn = true;  // 현재 플레이어 턴인지 체크
    public Button endTurnButton;  // 턴 종료 버튼

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (!champions.Contains(gameObject))
        {
            champions.Add(gameObject);
        }
        currentMoveRange = moveRange;

        // 턴 종료 버튼 설정
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndPlayerTurn);  // 턴 종료 버튼에 함수 연결
        }
    }

    private void Update()
    {
        if (isPlayerTurn)
        {
            if (isSelected && Input.GetMouseButtonDown(1))
            {
                DeselectChampion();
            }
        }
    }

    private void OnMouseDown()
    {
        // 적의 턴일 때는 챔피언과 상호작용할 수 없음
        if (!isPlayerTurn) return;

        SelectChampion();
        ShowMoveableTiles();
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        if (!isPlayerTurn) return;  // 적의 턴일 때는 이동 불가

        if (isSelected)
        {
            Vector2Int targetTilePos = FindTileAtPosition(newPosition);

            if (costs.ContainsKey(targetTilePos))
            {
                int moveCost = costs[targetTilePos];

                if (currentMoveRange >= moveCost)
                {
                    List<Vector2Int> path = ReconstructPath(targetTilePos);
                    StartCoroutine(MoveAlongPath(path, newPosition));
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

    private IEnumerator MoveAlongPath(List<Vector2Int> path, Vector3 finalPosition)
    {
        foreach (Vector2Int tile in path)
        {
            Vector3 tilePosition = Hex_Tile.allTiles[tile].transform.position;
            transform.position = tilePosition;
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.enabled = false;
        }

        transform.position = finalPosition;
        spriteRenderer.enabled = true;

        currentMoveRange -= costs[FindTileAtPosition(finalPosition)];
        ResetAllTiles();
        ShowMoveableTiles();
    }

    private List<Vector2Int> ReconstructPath(Vector2Int targetTile)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = targetTile;

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    public void ShowMoveableTiles()
    {
        costs.Clear();
        cameFrom.Clear();
        PriorityQueue<Vector2Int> frontier = new PriorityQueue<Vector2Int>();

        Vector2Int currentPos = FindTileAtPosition(transform.position);

        if (currentPos == null)
        {
            Debug.LogError("Champ의 위치에 해당하는 타일을 찾을 수 없습니다.");
            return;
        }

        costs[currentPos] = 0;
        frontier.Enqueue(currentPos, 0);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentCost = costs[current];

            if (currentCost > moveRange) continue;

            foreach (Vector2Int next in GetNeighbors(current))
            {
                if (Hex_Tile.allTiles.ContainsKey(next))
                {
                    int newCost = currentCost + Hex_Tile.allTiles[next].moveCost;

                    if (!costs.ContainsKey(next) || newCost < costs[next])
                    {
                        costs[next] = newCost;
                        frontier.Enqueue(next, newCost);
                        cameFrom[next] = current;

                        if (newCost <= currentMoveRange)
                        {
                            Hex_Tile.allTiles[next].HighlightTile();
                        }
                    }
                }
            }
        }
    }

    // 턴 종료 함수
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;  // 플레이어 턴 종료
        Debug.Log("플레이어 턴 종료. 적 턴 시작.");

        // 적의 턴 시작
        Enemy_Movement.StartEnemyTurn();  // 적의 턴 시작 (Enemy_Movement에서 관리)
    }

    // 적 턴 종료 후 플레이어 턴 시작
    public static void StartPlayerTurn()
    {
        isPlayerTurn = true;  // 플레이어 턴 시작
        Debug.Log("플레이어 턴 시작");

        // 모든 챔피언의 이동력 복구
        foreach (GameObject champ in champions)
        {
            Champ_Movement champMovement = champ.GetComponent<Champ_Movement>();
            if (champMovement != null)
            {
                champMovement.currentMoveRange = champMovement.moveRange;  // 이동력 복구
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