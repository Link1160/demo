using UnityEngine;
using System.Collections.Generic;

public class Priest : BattleUnit
{
    void Awake()
    {
        unitClass = UnitClass.Priest;
        maxHp = 4;
        hp = 4;
        attack = 1;
        magicDef = 0;
        moveRange = 1;
        skillCost = 2;
        attackRange = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
    }

    public override void NormalAttack(BattleUnit target)
    {
        if (target.camp == Camp.Player)
        {
            target.Heal(attack);
        }
        else
        {
            // 医师不攻击敌人，但为了简单，可造成1点伤害（根据文档医师无法造成伤害，这里按治疗处理）
            // 这里保持原设计：医师只能治疗队友
        }
        AddSpirit(1);
    }

    public override void UseSkill(BattleUnit target, List<BattleUnit> allUnits)
    {
        target.Heal(5);
        ResetSpirit();
    }
}