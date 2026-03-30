using UnityEngine;
using UnityEditor;

public class TileSetupTool : EditorWindow
{
    [MenuItem("Tools/批量添加Tile组件")]
    public static void AddTileToAllTiles()
    {
        string[] parentNames = { "pingdi", "bupingdi", "shui", "shu", "mogu", "Stones", "Blocks", "qitashijian", "qita", "guai" };

        foreach (string parentName in parentNames)
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent == null) continue;

            foreach (Transform child in parent.transform)
            {
                if (child.GetComponent<Tile>() == null)
                {
                    child.gameObject.AddComponent<Tile>();
                }
            }
        }

        Debug.Log("已为所有格子添加 Tile 组件！");
    }
}