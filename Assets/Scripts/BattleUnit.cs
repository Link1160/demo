using UnityEngine;
using System.Collections.Generic;

public enum UnitClass { Warrior, Mage, Priest }
public enum Camp { Player, Enemy }

public class BattleUnit : MonoBehaviour
{
    public Camp camp;
    public UnitClass unitClass;
    public int maxHp;
    public int hp;
    public int attack;
    public int magicDef;
    public int moveRange;
    public int spirit;
    public int skillCost;

    public Vector2Int gridPos;
    public List<Vector2Int> attackRange; // 相对坐标

    private List<GameObject> rangeIndicators = new List<GameObject>();

    public virtual void UseSkill(BattleUnit target, List<BattleUnit> allUnits) { }

    public virtual void NormalAttack(BattleUnit target)
    {
        int damage = attack;
        target.TakeDamage(damage);
        AddSpirit(1);
    }

    public virtual void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
        AddSpirit(1);
    }

    public virtual void Heal(int amount)
    {
        hp += amount;
        if (hp > maxHp) hp = maxHp;
    }

    public void AddSpirit(int amount)
    {
        spirit += amount;
        if (spirit > skillCost) spirit = skillCost;
    }

    public bool CanUseSkill() => spirit >= skillCost;
    public void ResetSpirit() => spirit = 0;

    protected virtual void Die()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }

    public void ShowMoveRange()
    {
        ClearRangeIndicators();
        List<Vector2Int> reachable = BattleManager.Instance.GetReachableTiles(gridPos, moveRange);
        foreach (var pos in reachable)
        {
            GameObject indicator = Instantiate(BattleManager.Instance.moveIndicatorPrefab, BattleManager.Instance.GetWorldPos(pos), Quaternion.identity);
            rangeIndicators.Add(indicator);
        }
    }

    public void ShowAttackRange()
    {
        ClearRangeIndicators();
        foreach (var offset in attackRange)
        {
            Vector2Int targetPos = gridPos + offset;
            if (BattleManager.Instance.IsInBoard(targetPos))
            {
                GameObject indicator = Instantiate(BattleManager.Instance.attackIndicatorPrefab, BattleManager.Instance.GetWorldPos(targetPos), Quaternion.identity);
                rangeIndicators.Add(indicator);
            }
        }
    }

    public void Heal(BattleUnit target, int amount)
    {
        target.hp += amount;
        if (target.hp > target.maxHp) target.hp = target.maxHp;
        Debug.Log($"{name} 治疗 {target.name} {amount} 点生命值");
    }

    public void ClearRangeIndicators()
    {
        foreach (var go in rangeIndicators) Destroy(go);
        rangeIndicators.Clear();
    }
    void OnMouseDown()
    {
        Debug.Log($"点击了 {name} ({gridPos})");
        BattleManager.Instance.OnUnitClicked(this);
    }
}