using UnityEngine;
using UnityEditor;

public class FixTileScale : EditorWindow
{
    [MenuItem("Tools/自动调整格子缩放")]
    static void Fix()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile t in tiles)
        {
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float ppu = sr.sprite.pixelsPerUnit;
                float originalWidth = sr.sprite.rect.width / ppu;
                float targetWidth = 1f; // 你的格子期望宽度
                float scale = targetWidth / originalWidth;
                t.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
        Debug.Log("缩放已调整");
    }
}
