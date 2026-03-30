using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;   // 格子坐标（自动计算）
    public TileType type;        // 格子类型

    void Awake()
    {
        // 自动推断类型（仅当未手动设置时）
        if (type == TileType.None && transform.parent != null)
        {
            string parentName = transform.parent.name;
            switch (parentName)
            {
                case "pingdi":
                    type = TileType.Ground;
                    break;
                case "shui":
                    type = TileType.Water;
                    break;
                case "shu":
                case "mogu":
                case "Stones":
                    type = TileType.Wall;
                    break;
                case "qitashijian":
                case "qita":
                    type = TileType.Event;      // 先统一为事件，后续再区分具体事件
                    break;
                case "guai":
                    type = TileType.Battle;
                    break;
                default:
                    type = TileType.Ground;     // 保险默认
                    break;
            }
        }

        // 自动计算坐标（如果没手动填）
        if (gridPos == Vector2Int.zero && transform.position != Vector3.zero)
        {
            gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        }
    }
}
