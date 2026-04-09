using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    //private Vector2Int? pendingEventPos;           // 移动完成后需要触发的事件格子坐标
    private Vector2Int? pendingEventPosForDisplay; // 仅用于显示（可选）
    private Vector2Int? pendingEventPos = null; // 存储待触发的事件格子坐标
    public enum TileEffectResult
    {
        Continue,   // 不中断移动
        Stop        // 中断移动，停在当前格子
    }
    public Vector2Int gridPos;
    public GameObject pathIndicatorPrefab;   // 路径指示器预制体（在 Inspector 中拖入）

    private MapManager map;
    private List<GameObject> currentPathIndicators = new List<GameObject>();
    private List<Vector2Int> currentPath = null;
    private bool isMoving = false;

    void Start()
    {
        map = MapManager.Instance;
        if (map == null)
        {
            Debug.LogError("MapManager 未找到！");
            return;
        }

        // 根据当前世界坐标找到起始格子
        Tile startTile = FindClosestTile(transform.position);
        if (startTile != null)
        {
            gridPos = startTile.gridPos;
            transform.position = startTile.transform.position;
        }
        else
        {
            Debug.LogError("找不到起始格子！");
        }
    }

    void Update()
    {
        // 如果正在显示事件UI，禁止地图操作
        if (GameManager.Instance.IsInEvent) return;
        if (isMoving) return;   // 移动中不允许点击

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile == null) return;

                Vector2Int clickedPos = clickedTile.gridPos;
                Vector2Int targetPos = clickedPos;
                Vector2Int? eventPos = null;

                // 如果点击的格子不可走（事件/战斗/墙），则尝试寻找相邻的可走格子
                if (!map.IsWalkable(clickedPos))
                {
                    // 检查是否已经站在相邻格
                    if (IsAdjacent(gridPos, clickedPos))
                    {
                        // 直接触发事件，不移动
                        Debug.Log("已在事件格相邻，直接触发");
                        ClearPathIndicators();
                        currentPath = null;
                        TriggerTileEffect(clickedPos);
                        return;
                    }

                    // 否则寻找最近的相邻可走格子
                    Vector2Int? neighbor = FindAdjacentWalkableTile(clickedPos);
                    if (neighbor == null)
                    {
                        Debug.Log("目标格子不可走，且周围没有可走格子");
                        ClearPathIndicators();
                        currentPath = null;
                        return;
                    }
                    targetPos = neighbor.Value;
                    eventPos = clickedPos; // 记录真正要触发的事件格子
                }

                // 如果已经有路径且点击的是同一个目标格子，则执行移动
                if (currentPath != null && currentPath.Count > 0 && currentPath[currentPath.Count - 1] == targetPos)
                {
                    Debug.Log("确认移动，开始走路径...");
                    pendingEventPos = eventPos; // 存储待触发事件
                    StartCoroutine(MoveAlongPath(currentPath));
                    ClearPathIndicators();
                    currentPath = null;
                }
                else
                {
                    // 重新计算路径
                    List<Vector2Int> path = map.FindPath(gridPos, targetPos);
                    if (path != null && path.Count > 0)
                    {
                        Debug.Log($"找到路径，长度 {path.Count}");
                        currentPath = path;
                        ShowPathIndicators(path);
                        // 注意：不立即移动，等待二次点击
                    }
                    else
                    {
                        Debug.Log("没有路径");
                        ClearPathIndicators();
                        currentPath = null;
                    }
                }
            }
            else
            {
                // 点击空白区域清除路径
                ClearPathIndicators();
                currentPath = null;
                pendingEventPos = null;
            }
        }
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private Vector2Int? FindAdjacentWalkableTile(Vector2Int center)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int? best = null;
        int bestDist = int.MaxValue;

        foreach (var dir in dirs)
        {
            Vector2Int neighbor = center + dir;
            if (map.IsWalkable(neighbor))
            {
                int dist = Mathf.Abs(neighbor.x - gridPos.x) + Mathf.Abs(neighbor.y - gridPos.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = neighbor;
                }
            }
        }
        return best;
    }

    void ShowPathIndicators(List<Vector2Int> path)
    {
        ClearPathIndicators();
        foreach (Vector2Int pos in path)
        {
            Tile tile = map.GetTile(pos);
            if (tile != null)
            {
                GameObject indicator = Instantiate(pathIndicatorPrefab, tile.transform.position, Quaternion.identity);
                currentPathIndicators.Add(indicator);
            }
        }
    }

    /// <summary>
    /// 清除所有路径指示器
    /// </summary>
    void ClearPathIndicators()
    {
        foreach (GameObject go in currentPathIndicators)
        {
            if (go != null) Destroy(go);
        }
        currentPathIndicators.Clear();
    }

    /// <summary>
    /// 沿路径逐步移动
    /// </summary>
    private System.Collections.IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isMoving = true;

        foreach (Vector2Int step in path)
        {
            // 检查下一步是否可走
            if (!map.IsWalkable(step))
            {
                Debug.Log($"路径中格子 {step} 变得不可走，移动中断");
                break;
            }

            Tile targetTile = map.GetTile(step);
            if (targetTile == null) break;

            // 移动
            gridPos = step;
            transform.position = targetTile.transform.position;

            // 每步触发格子效果（仅针对 Water，不中断移动）
            if (targetTile.type == TileType.Water)
            {
                Debug.Log("踩到水，扣血！");
                GameManager.Instance.TakeDamage(4); // 调用统一伤害处理
            }

            // 注意：不再每步触发格子效果
            yield return new WaitForSeconds(0.2f);
        }

        isMoving = false;

        // 移动结束后，如果有待触发的事件格子，则触发事件
        if (pendingEventPos != null)
        {
            TriggerTileEffect(pendingEventPos.Value);
            pendingEventPos = null;
        }

        Debug.Log("移动结束");
    }

    TileEffectResult TriggerTileEffect(Vector2Int pos)
    {
        Tile t = map.GetTile(pos);
        if (t == null) return TileEffectResult.Continue;

        switch (t.type)
        {
            case TileType.Water:
                Debug.Log("踩到水，扣血！");
                GameManager.Instance.TakeDamage(4);
                return TileEffectResult.Continue;

            case TileType.Event:
                MultiStepEvent multi = map.GetTile(pos).GetComponent<MultiStepEvent>();
                if (multi != null) multi.Trigger();
                else
                {
                    SimpleEvent simple = map.GetTile(pos).GetComponent<SimpleEvent>();
                    if (simple != null) simple.Trigger();
                    else
                    {
                        BarracksEvent barracks = map.GetTile(pos).GetComponent<BarracksEvent>();
                        if (barracks != null) barracks.Trigger();
                    }
                }
                return TileEffectResult.Stop;

            case TileType.Battle:
                Debug.Log("进入战斗");
                // 开始战斗
                StartBattle(t);
                return TileEffectResult.Stop;   // 中断移动

            default:
                return TileEffectResult.Continue;
        }
    }
    void ShowEventUI(Tile eventTile)
    {
        // 这里弹出事件面板
        // 例如：通过 Canvas 上的事件面板，显示文案和选项
        // 处理完成后，不需要额外操作，因为角色已经停在这里
        Debug.Log($"显示事件，格子坐标 {eventTile.gridPos}");
        // 你可以在这里实例化事件 UI 预制体，并传入事件数据
    }

    void StartBattle(Tile battleTile)
    {
        // 切换到战斗场景，战斗结束后返回大地图
        // 注意：场景切换时，当前玩家的位置已经在这个战斗格上，返回后需要恢复状态
        Debug.Log($"进入战斗，格子坐标 {battleTile.gridPos}");
        // 这里可以调用 SceneManager.LoadScene 加载战斗场景
    }

    /// <summary>
    /// 查找距离世界坐标最近的格子（用于初始化）
    /// </summary>
    Tile FindClosestTile(Vector3 worldPos)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        Tile closest = null;
        float minDist = float.MaxValue;
        foreach (Tile t in allTiles)
        {
            float dist = Vector3.Distance(worldPos, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = t;
            }
        }
        return closest;
    }

    public void MoveTo(Vector2Int targetPos)
    {
        Tile targetTile = map.GetTile(targetPos);
        if (targetTile != null)
        {
            gridPos = targetPos;
            transform.position = targetTile.transform.position;
            // 不触发格子效果，因为这是事件选项导致的移动
        }
        else
        {
            Debug.LogError($"目标格子 {targetPos} 不存在");
        }
    }

}