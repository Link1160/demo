using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CampusRunEvent : MonoBehaviour
{
    public GameObject bikeObject; // 自行车图片对象，在 Inspector 中拖入

    public void Trigger()
    {
        // 如果已经骑过车且格子已变为可走，则事件已彻底结束
        if (GameManager.Instance.hasBike && GetComponent<Tile>().type == TileType.Ground)
            return;

        GameManager.Instance.SetInEvent(true);

        // 如果已经接取了校园跑任务（isInCampusRun 为 true），则直接显示自行车选择面板
        if (GameManager.Instance.isInCampusRun)
        {
            ShowBikeChoice();
        }
        else
        {
            ShowFirstPanel();
        }
    }

    private void ShowFirstPanel()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "是否在此接取校园跑任务？");
        float y = -150f;
        CreateButton(panel, "不跑，以后再说", () => {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
        y -= 60f;
        CreateButton(panel, "开始校园跑", () => {
            GameManager.Instance.isInCampusRun = true;
            Destroy(panel);
            ShowBikeChoice();
        }, y);
    }

    private void ShowBikeChoice()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "你要骑自行车吗？");
        float y = -150f;
        CreateButton(panel, "我不骑！", () => {
            GameManager.Instance.hasBike = false;
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
        y -= 60f;
        CreateButton(panel, "骑！骑的就是自行车！", () => {
            GameManager.Instance.hasBike = true;
            if (bikeObject != null) Destroy(bikeObject);
            Tile tile = GetComponent<Tile>();
            if (tile != null) tile.type = TileType.Ground;
            Destroy(this); // 移除自身脚本，不再触发
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
    }

    // ---------- UI 辅助方法 ----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;
        GameObject panel = new GameObject("CampusRunPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return panel;
    }

    private void ShowDescription(GameObject panel, string desc)
    {
        GameObject go = new GameObject("Desc");
        go.transform.SetParent(panel.transform, false);
        Text txt = go.AddComponent<Text>();
        txt.text = desc;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        txt.font = font;
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform rect = txt.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.3f);
        rect.anchorMax = new Vector2(0.9f, 0.7f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void CreateButton(GameObject panel, string label, UnityEngine.Events.UnityAction onClick, float yPos)
    {
        GameObject go = new GameObject("Button");
        go.transform.SetParent(panel.transform, false);
        Button btn = go.AddComponent<Button>();
        Image img = go.AddComponent<Image>();
        img.color = Color.gray;
        Text txt = CreateText(go.transform, label, 20);
        txt.rectTransform.anchoredPosition = Vector2.zero;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = new Vector2(0, yPos);
        btn.onClick.AddListener(onClick);
    }

    private Text CreateText(Transform parent, string text, int fontSize)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text txt = go.AddComponent<Text>();
        txt.text = text;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        txt.font = font;
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        RectTransform rect = txt.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = Vector2.zero;
        return txt;
    }
}