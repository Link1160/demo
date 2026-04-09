using UnityEngine;
using UnityEditor;

public class RandomTreeSpriteReplacer : EditorWindow
{
    [MenuItem("Tools/随机替换树林图片")]
    static void ShowWindow()
    {
        GetWindow<RandomTreeSpriteReplacer>("随机替换树林");
    }

    public Sprite treeSprite1;
    public Sprite treeSprite2;
    public string parentName = "shu";

    void OnGUI()
    {
        GUILayout.Label("随机替换树林格子图片", EditorStyles.boldLabel);
        parentName = EditorGUILayout.TextField("父物体名称", parentName);
        treeSprite1 = (Sprite)EditorGUILayout.ObjectField("树林图片1", treeSprite1, typeof(Sprite), false);
        treeSprite2 = (Sprite)EditorGUILayout.ObjectField("树林图片2", treeSprite2, typeof(Sprite), false);

        if (GUILayout.Button("随机替换"))
        {
            ReplaceRandom();
        }
    }

    void ReplaceRandom()
    {
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
        {
            Debug.LogError($"找不到父物体: {parentName}");
            return;
        }

        SpriteRenderer[] renderers = parent.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            Sprite selected = Random.Range(0, 2) == 0 ? treeSprite1 : treeSprite2;
            sr.sprite = selected;
        }
        Debug.Log($"已随机替换 {renderers.Length} 个树格子");
    }
}