using UnityEngine;
using UnityEditor;

public class CleanTileColliders : EditorWindow
{
    [MenuItem("Tools/清理并修复格子碰撞器")]
    static void CleanAndFix()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        int fixedCount = 0;
        foreach (Tile t in tiles)
        {
            BoxCollider2D[] colliders = t.GetComponents<BoxCollider2D>();
            if (colliders.Length > 1)
            {
                // 保留第一个，删除其余
                for (int i = 1; i < colliders.Length; i++)
                {
                    DestroyImmediate(colliders[i]);
                }
                Debug.Log($"删除了 {colliders.Length - 1} 个多余碰撞器 on {t.name}");
            }
            // 确保有一个碰撞器
            BoxCollider2D col = t.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = t.gameObject.AddComponent<BoxCollider2D>();
            }
            // 根据 Sprite 自动调整碰撞器大小和偏移
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float ppu = sr.sprite.pixelsPerUnit;
                Vector2 size = new Vector2(sr.sprite.rect.width / ppu, sr.sprite.rect.height / ppu);
                Vector2 pivot = sr.sprite.pivot / ppu;
                Vector2 center = pivot - size * 0.5f;
                col.size = size;
                col.offset = center;
            }
            else
            {
                // 默认大小（无图片时）
                col.size = new Vector2(1, 1);
                col.offset = Vector2.zero;
            }
            fixedCount++;
        }
        Debug.Log($"已清理并修复 {fixedCount} 个格子");
    }
}
