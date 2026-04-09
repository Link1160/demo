using UnityEngine;
using UnityEditor;

public class FixTileCollider : EditorWindow
{
    [MenuItem("Tools/修复所有格子的碰撞器")]
    static void FixAll()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        int count = 0;
        foreach (Tile t in tiles)
        {
            BoxCollider2D col = t.GetComponent<BoxCollider2D>();
            if (col == null) continue;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                // 根据 Sprite 的实际边界自动调整碰撞器大小和偏移
                Bounds bounds = sr.sprite.bounds;
                col.size = bounds.size;
                col.offset = bounds.center;
                count++;
            }
        }
        Debug.Log($"已修复 {count} 个格子的碰撞器");
    }
}
