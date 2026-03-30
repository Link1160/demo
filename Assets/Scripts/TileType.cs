// TileType.cs
public enum TileType
{
    None,       // 未设置
    Ground,     // 可走
    Wall,       // 不可走
    Water,      // 水上，走上去扣血
    Event,      // 事件格（弹出文案、奖励）
    Battle      // 战斗格
}
