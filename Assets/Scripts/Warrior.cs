using UnityEngine;
using System.Collections.Generic;

public class Warrior : BattleUnit
{
    void Awake()
    {
        unitClass = UnitClass.Warrior;
        maxHp = 8;
        hp = 8;
        attack = 2;
        magicDef = 1;
        moveRange = 3;
        skillCost = 5;
        attackRange = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
    }

    public override void UseSkill(BattleUnit target, List<BattleUnit> allUnits)
    {
        target.TakeDamage(attack * 2);
        ResetSpirit();
    }
}