using UnityEngine;

public class BattleTile : MonoBehaviour
{
    public Vector2Int gridPos;   // 格子坐标 (x, y)，x: 0~7, y: 0~4

    void Start()
    {
        // 自动根据世界坐标计算 gridPos（如果没手动设置）
        if (gridPos == Vector2Int.zero)
        {
            gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        }
    }

    void OnMouseDown()
    {
        // 检测鼠标下方是否有棋子（通过射线检测）
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponent<BattleUnit>() != null)
        {
            // 如果有棋子，不处理格子点击，让棋子的 OnMouseDown 处理
            return;
        }

        Debug.Log($"点击格子 {gridPos}");
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnTileClicked(this);
        else
            Debug.LogError("BattleManager.Instance 为空！");
    }
}
