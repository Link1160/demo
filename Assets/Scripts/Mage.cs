using UnityEngine;
using System.Collections.Generic;

public class Mage : BattleUnit
{
    void Awake()
    {
        unitClass = UnitClass.Mage;
        maxHp = 5;
        hp = 5;
        attack = 2;
        magicDef = 0;
        moveRange = 2;
        skillCost = 4;
        attackRange = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(0, 2), new Vector2Int(0, -2), new Vector2Int(2, 0), new Vector2Int(-2, 0)
        };
    }

    public override void UseSkill(BattleUnit target, List<BattleUnit> allUnits)
    {
        target.TakeDamage(attack * 2);
        ResetSpirit();
    }
}