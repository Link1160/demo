using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject warriorPrefab;
    public GameObject magePrefab;
    public GameObject priestPrefab;
    public GameObject moveIndicatorPrefab;    // 移动范围指示器（半透明绿色方块）
    public GameObject attackIndicatorPrefab;  // 攻击范围指示器（半透明红色方块）

    private BattleTile[,] tiles;
    private List<BattleUnit> playerUnits = new List<BattleUnit>();
    private List<BattleUnit> enemyUnits = new List<BattleUnit>();

    private List<BattleUnit> unactedUnits = new List<BattleUnit>();  // 未行动的我方单位
    private BattleUnit currentActingUnit;   // 当前正在行动的单位

    private bool isPlayerTurn = true;
    private BattleUnit selectedUnit;
    private bool isMovingPhase = true;   // true=移动阶段, false=攻击阶段


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateBoard();
        PlaceUnits();
        StartPlayerTurn();   // 开始第一个玩家回合
    }

    void GenerateBoard()
    {
        int width = 8;
        int height = 5;
        tiles = new BattleTile[width, height];
        float tileSize = 1f;
        float startX = 0f;
        float startY = 0f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(startX + x * tileSize, startY + y * tileSize, 0);
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity);
                tileObj.transform.SetParent(this.transform);
                BattleTile tile = tileObj.GetComponent<BattleTile>();
                tile.gridPos = new Vector2Int(x, y);
                tiles[x, y] = tile;
            }
        }

        // 调整相机位置（可选）
        Camera.main.transform.position = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, -10);
    }

    void PlaceUnits()
    {
        // 我方单位（第1-2列）
        CreateUnit(warriorPrefab, Camp.Player, new Vector2Int(0, 2));
        CreateUnit(magePrefab, Camp.Player, new Vector2Int(1, 2));
        CreateUnit(priestPrefab, Camp.Player, new Vector2Int(0, 1));

        // 敌方单位：普通（第6列）、精英（第7列）、领袖（第8列）
        // 为了区分强度，可以在创建后修改属性
        BattleUnit normal = CreateUnit(warriorPrefab, Camp.Enemy, new Vector2Int(6, 2));
        BattleUnit elite = CreateUnit(warriorPrefab, Camp.Enemy, new Vector2Int(7, 2));
        BattleUnit leader = CreateUnit(warriorPrefab, Camp.Enemy, new Vector2Int(7, 3)); // 领袖放在不同行

        // 设置不同属性（示例）
        normal.maxHp = 6; normal.hp = 6; normal.attack = 2;
        elite.maxHp = 10; elite.hp = 10; elite.attack = 3;
        leader.maxHp = 15; leader.hp = 15; leader.attack = 4;
    }

    BattleUnit CreateUnit(GameObject prefab, Camp camp, Vector2Int pos)
    {
        GameObject obj = Instantiate(prefab, GetWorldPos(pos), Quaternion.identity);
        BattleUnit unit = obj.GetComponent<BattleUnit>();
        unit.camp = camp;
        unit.gridPos = pos;
        if (camp == Camp.Player) playerUnits.Add(unit);
        else enemyUnits.Add(unit);
        return unit;
    }

    // 世界坐标转格子世界坐标（假设格子边长为1）
    public Vector3 GetWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, gridPos.y, 0);
    }

    public bool IsInBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 5;
    }

    // BFS 获取可达格子
    public List<Vector2Int> GetReachableTiles(Vector2Int start, int range)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();
        queue.Enqueue(start);
        dist[start] = 0;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            int d = dist[cur];
            if (d >= range) continue;
            foreach (var dir in dirs)
            {
                Vector2Int next = cur + dir;
                if (IsInBoard(next) && !IsOccupied(next) && !dist.ContainsKey(next))
                {
                    dist[next] = d + 1;
                    queue.Enqueue(next);
                    result.Add(next);
                }
            }
        }
        return result;
    }

    bool IsOccupied(Vector2Int pos)
    {
        foreach (var u in playerUnits) if (u.gridPos == pos) return true;
        foreach (var u in enemyUnits) if (u.gridPos == pos) return true;
        return false;
    }

    // 点击格子回调（由 BattleTile 调用）
    public void OnTileClicked(BattleTile tile)
    {
        if (!isPlayerTurn) return;
        if (currentActingUnit == null) return;

        if (isMovingPhase)
        {
            Vector2Int targetPos = tile.gridPos;
            List<Vector2Int> reachable = GetReachableTiles(currentActingUnit.gridPos, currentActingUnit.moveRange);

            // 调试输出
            Debug.Log($"当前单位位置: {currentActingUnit.gridPos}, 移动范围: {currentActingUnit.moveRange}");
            Debug.Log($"可达格子数量: {reachable.Count}");
            foreach (var pos in reachable) Debug.Log($"可达: {pos}");

            if (reachable.Contains(targetPos))
            {
                MoveUnit(currentActingUnit, targetPos);
                currentActingUnit.ClearRangeIndicators();
                isMovingPhase = false;
                currentActingUnit.ShowAttackRange();
            }
            else
            {
                Debug.Log($"目标格子 {targetPos} 不可达");
            }
        }
        else
        {
            // ========= 攻击阶段 =========
            Vector2Int pos = tile.gridPos;
            BattleUnit targetUnit = GetUnitAt(pos, Camp.Player);
            if (targetUnit == null) targetUnit = GetUnitAt(pos, Camp.Enemy);

            if (targetUnit != null)
            {
                // 治疗（仅医师且目标是友方）
                if (currentActingUnit.unitClass == UnitClass.Priest && targetUnit.camp == Camp.Player)
                {
                    if (IsInAttackRange(currentActingUnit, targetUnit))
                    {
                        currentActingUnit.Heal(targetUnit, currentActingUnit.attack);
                        EndUnitAction();
                    }
                    else
                    {
                        Debug.Log("治疗目标不在范围内");
                    }
                }
                // 攻击敌方
                else if (targetUnit.camp == Camp.Enemy && IsInAttackRange(currentActingUnit, targetUnit))
                {
                    currentActingUnit.NormalAttack(targetUnit);
                    EndUnitAction();
                }
                else
                {
                    Debug.Log("不能攻击/治疗该单位（不在范围内或职业不符）");
                }
            }
            else
            {
                // 格子为空，跳过行动
                Debug.Log("攻击阶段点击空地，跳过");
                EndUnitAction();
            }
        }
    }

    BattleUnit GetUnitAt(Vector2Int pos, Camp camp)
    {
        List<BattleUnit> list = (camp == Camp.Player) ? playerUnits : enemyUnits;
        foreach (var u in list)
            if (u.gridPos == pos) return u;
        return null;
    }

    bool IsInAttackRange(BattleUnit attacker, BattleUnit target)
    {
        Vector2Int diff = target.gridPos - attacker.gridPos;
        return attacker.attackRange.Contains(diff);
    }

    void MoveUnit(BattleUnit unit, Vector2Int newPos)
    {
        unit.gridPos = newPos;
        unit.transform.position = GetWorldPos(newPos);
    }
    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        unactedUnits.Clear();
        foreach (var unit in playerUnits) unactedUnits.Add(unit);
        currentActingUnit = null;
        isMovingPhase = true;
        Debug.Log("玩家回合开始");
    }
    void EndPlayerTurn()
    {
        isPlayerTurn = false;
        StartCoroutine(EnemyTurn());
    }

    System.Collections.IEnumerator EnemyTurn()
    {
        // ... 敌人行动代码 ...
        yield return new WaitForSeconds(1f);
        // 敌方回合结束，开始玩家回合
        StartPlayerTurn();
    }

    BattleUnit FindClosestPlayer(BattleUnit enemy)
    {
        BattleUnit closest = null;
        float minDist = float.MaxValue;
        foreach (var p in playerUnits)
        {
            float d = Vector2Int.Distance(enemy.gridPos, p.gridPos);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }
        return closest;
    }

    Vector2Int GetAdjacentTile(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = new Vector2Int(Mathf.Clamp(to.x - from.x, -1, 1), Mathf.Clamp(to.y - from.y, -1, 1));
        Vector2Int candidate = from + dir;
        if (IsInBoard(candidate) && !IsOccupied(candidate))
            return candidate;
        return from;
    }

    public void RemoveUnit(BattleUnit unit)
    {
        // 从单位列表中移除
        if (unit.camp == Camp.Player)
        {
            playerUnits.Remove(unit);
            // 如果该单位是当前行动单位，则结束其行动
            if (currentActingUnit == unit)
            {
                currentActingUnit.ClearRangeIndicators();
                currentActingUnit = null;
                // 从未行动列表中移除（如果还在）
                if (unactedUnits.Contains(unit))
                    unactedUnits.Remove(unit);
                // 如果还有未行动单位，继续；否则结束玩家回合
                if (unactedUnits.Count > 0)
                {
                    // 提示选择下一个单位
                    isMovingPhase = true;
                    Debug.Log("当前单位死亡，请选择下一个单位");
                }
                else
                {
                    EndPlayerTurn();
                }
            }
            else
            {
                // 如果不是当前行动单位，但还在未行动列表中，也要移除
                if (unactedUnits.Contains(unit))
                    unactedUnits.Remove(unit);
            }
        }
        else
        {
            enemyUnits.Remove(unit);
        }

        // 胜负判定
        if (playerUnits.Count == 0)
        {
            Debug.Log("战斗失败");
            // 可以加载失败界面或返回大地图
        }
        else if (enemyUnits.Count == 0)
        {
            Debug.Log("战斗胜利");
            // 胜利后返回大地图，并奖励
        }
    }
    public void OnUnitClicked(BattleUnit unit)
    {
        if (!isPlayerTurn) return;

        if (unit.camp == Camp.Player)
        {
            // 如果该单位本回合已行动过，忽略
            if (!unactedUnits.Contains(unit)) return;

            // ========= 攻击阶段：只能操作当前选中的单位 =========
            if (!isMovingPhase)
            {
                if (currentActingUnit == unit)
                {
                    // 刷新攻击范围（可选）
                    currentActingUnit.ShowAttackRange();
                }
                else
                {
                    Debug.Log("当前单位正在攻击阶段，不能切换其他单位");
                }
                return;
            }

            // ========= 移动阶段 =========
            if (currentActingUnit == null)
            {
                // 第一次选中单位
                currentActingUnit = unit;
                currentActingUnit.ShowMoveRange();
            }
            else if (currentActingUnit == unit)
            {
                // 再次点击自己，跳过移动
                currentActingUnit.ClearRangeIndicators();
                isMovingPhase = false;
                currentActingUnit.ShowAttackRange();
            }
            else if (currentActingUnit != unit && unactedUnits.Contains(unit))
            {
                // 切换选中另一个未行动单位
                currentActingUnit.ClearRangeIndicators();
                currentActingUnit = unit;
                currentActingUnit.ShowMoveRange();
            }
        }
        else
        {
            // 点击敌方单位：只有攻击阶段且有选中的我方单位才可攻击
            if (!isMovingPhase && currentActingUnit != null && IsInAttackRange(currentActingUnit, unit))
            {
                currentActingUnit.NormalAttack(unit);
                EndUnitAction();
            }
            else
            {
                Debug.Log("不能攻击或不在攻击范围内");
            }
        }
    }
    public void SkipMove()
    {
        if (!isPlayerTurn || currentActingUnit == null || !isMovingPhase) return;
        // 清除移动范围，直接进入攻击阶段
        currentActingUnit.ClearRangeIndicators();
        isMovingPhase = false;
        currentActingUnit.ShowAttackRange();
    }
    void EndUnitAction()
    {
        // 清除当前单位的指示器
        if (currentActingUnit != null)
            currentActingUnit.ClearRangeIndicators();
        // 从未行动列表中移除
        unactedUnits.Remove(currentActingUnit);
        currentActingUnit = null;

        // 检查是否还有未行动的单位
        if (unactedUnits.Count == 0)
        {
            // 所有单位行动完毕，结束玩家回合
            EndPlayerTurn();
        }
        else
        {
            // 还有单位未行动，等待玩家选择下一个
            isMovingPhase = true;
            Debug.Log("请选择下一个行动的单位");
        }
    }
}