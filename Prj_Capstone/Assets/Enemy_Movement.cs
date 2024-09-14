using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public int moveRange = 3;  // 적의 이동 범위
    private Dictionary<Vector2Int, int> costs = new Dictionary<Vector2Int, int>();  // Dijkstra 비용 저장
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();  // 경로 추적

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public static void StartEnemyTurn()
    {
        // 적의 턴을 처리하는 로직
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  // 적을 모두 찾음

        foreach (GameObject enemy in enemies)
        {
            Enemy_Movement enemyMovement = enemy.GetComponent<Enemy_Movement>();
            if (enemyMovement != null)
            {
                enemyMovement.MoveTowardsClosestChampion();  // 각 적마다 가까운 챔피언을 향해 이동
            }
        }

        // 적 턴이 끝나면 플레이어 턴으로 전환
        Champ_Movement.StartPlayerTurn();
    }

    private void MoveTowardsClosestChampion()
    {
        // 1. 가장 가까운 플레이어 챔피언을 찾음
        GameObject closestChampion = FindClosestChampion();

        if (closestChampion != null)
        {
            // 2. Dijkstra 알고리즘으로 최단 경로 계산
            Vector3 targetPosition = closestChampion.transform.position;
            Vector2Int targetTilePos = FindTileAtPosition(targetPosition);
            List<Vector2Int> path = CalculateShortestPathTo(targetTilePos);

            // 3. 경로를 따라 이동
            if (path.Count > 0)
            {
                StartCoroutine(MoveAlongPath(path));
            }
        }
    }

    private GameObject FindClosestChampion()
    {
        GameObject closestChampion = null;
        float closestDistance = float.MaxValue;

        // Champ_Movement.champions 리스트를 순회하면서 가장 가까운 챔피언을 찾음
        foreach (GameObject champion in Champ_Movement.champions)
        {
            float distance = Vector3.Distance(transform.position, champion.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestChampion = champion;
            }
        }

        return closestChampion;
    }

    private List<Vector2Int> CalculateShortestPathTo(Vector2Int targetTilePos)
    {
        costs.Clear();
        cameFrom.Clear();
        PriorityQueue<Vector2Int> frontier = new PriorityQueue<Vector2Int>();

        Vector2Int currentPos = FindTileAtPosition(transform.position);
        if (currentPos == null)
        {
            Debug.LogError("적의 위치에 해당하는 타일을 찾을 수 없습니다.");
            return new List<Vector2Int>();
        }

        costs[currentPos] = 0;
        frontier.Enqueue(currentPos, 0);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentCost = costs[current];

            if (currentCost > moveRange) continue;  // 이동 범위를 초과하는 타일은 무시

            if (current == targetTilePos) break;  // 목표 타일에 도달했으면 중단

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
                    }
                }
            }
        }

        // 3. 경로를 추적해서 반환
        return ReconstructPath(targetTilePos);
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

    // 코루틴을 이용해 경로를 따라 이동
    private IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        foreach (Vector2Int tile in path)
        {
            Vector3 tilePosition = Hex_Tile.allTiles[tile].transform.position;
            transform.position = tilePosition;

            // 0.2초 대기 후 다음 타일로 이동
            yield return new WaitForSeconds(0.2f);
        }
    }

    // 적의 위치에서 가장 가까운 타일을 찾는 함수
    private Vector2Int FindTileAtPosition(Vector3 position)
    {
        float closestDistance = float.MaxValue;
        Vector2Int closestTile = Vector2Int.zero;

        foreach (var tile in Hex_Tile.allTiles)
        {
            float distance = Vector3.Distance(position, tile.Value.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile.Key;
            }
        }

        return closestTile;
    }

    // 인접한 타일을 찾는 함수 (육각형 타일 기준)
    private List<Vector2Int> GetNeighbors(Vector2Int tilePos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

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
}