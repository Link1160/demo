using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubwayEvent : MonoBehaviour
{
    public Vector2Int targetPos;   // 传送目标坐标（另一个地铁站的位置）

    public void Trigger()
    {
        GameManager.Instance.SetInEvent(true);
        ShowPanel();
    }

    void ShowPanel()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "2号线地铁站，通向另一站");
        float y = -150f;
        CreateButton(panel, "离开", () => {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
        y -= 60f;
        CreateButton(panel, "坐地铁 (hp-10)", () => {
            if (GameManager.Instance.health <= 10)
            {
                // 血量不足，提示无法传送（可选）
                Debug.Log("血量不足，无法坐地铁");
                GameManager.Instance.SetInEvent(false);
                Destroy(panel);
                return;
            }
            GameManager.Instance.TakeDamage(10);
            // 传送玩家
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.TeleportTo(targetPos);
            }
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
            StartCoroutine(ShowMessage("大学生的校车 be like..."));
        }, y);
    }

    IEnumerator ShowMessage(string msg)
    {
        GameManager.Instance.SetInEvent(true);
        GameObject panel = CreatePanel();
        Text txt = CreateText(panel.transform, msg, 24);
        txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }

    // ---------- UI 辅助方法（复用）----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;
        GameObject panel = new GameObject("SubwayPanel");
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
        // 使用 GameManager 中的中文字体
        Font font = GameManager.Instance.chineseFont;
        //Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
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
        // 使用 GameManager 中的中文字体
        Font font = GameManager.Instance.chineseFont;
        //Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
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