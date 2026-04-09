using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    private Dictionary<Vector2Int, Tile> tileDict;
    private int minX, maxX, minY, maxY;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GatherTiles();
    }

    void GatherTiles()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        tileDict = new Dictionary<Vector2Int, Tile>();

        Debug.Log($"找到 {allTiles.Length} 个格子");

        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;

        foreach (Tile t in allTiles)
        {
            Vector2Int pos = t.gridPos;
            if (!tileDict.ContainsKey(pos))
            {
                tileDict.Add(pos, t);
            }
            else
            {
                Debug.LogWarning($"重复坐标 {pos}，已忽略");
            }

            // 更新边界
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        Debug.Log($"地图范围：X [{minX}, {maxX}]，Y [{minY}, {maxY}]");
    }

    public Tile GetTile(Vector2Int pos)
    {
        tileDict.TryGetValue(pos, out Tile t);
        return t;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        Tile t = GetTile(pos);
        if (t == null) return false;
        // 只有 Ground 和 Water 可走（Water 可走但要扣血）
        return t.type == TileType.Ground || t.type == TileType.Water;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        // 起点终点相同，直接返回空列表（不需要移动）
        if (start == end) return new List<Vector2Int>();

        // 检查终点是否可走（终点为 Wall 则不可达）
        Tile endTile = GetTile(end);
        if (endTile == null || !IsWalkable(end))
        {
            Debug.Log($"终点 {end} 不可走");
            return null;
        }

        // BFS 数据结构
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        // 四个方向（上下左右）
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                // 重建路径
                return ReconstructPath(cameFrom, start, end);
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // 检查邻居是否在范围内且可走且未访问
                if (IsWalkable(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 没有找到路径
        Debug.Log($"无法从 {start} 走到 {end}");
        return null;
    }

    /// <summary>
    /// 从 cameFrom 字典重建路径（不含起点，含终点）
    /// </summary>
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();   // 变成从起点到终点的顺序
        return path;
    }
}