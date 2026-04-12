using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BluePlanetEvent : MonoBehaviour
{
    public Sprite brokenSprite;   // 破损贴图
    public Sprite fixedSprite;    // 修复后贴图
    private bool isFixed = false;
    private SpriteRenderer overlaySprite;

    void Start()
    {
        // 创建覆盖图片的子物体（如果不存在）
        Transform overlay = transform.Find("BluePlanetOverlay");
        if (overlay == null)
        {
            GameObject obj = new GameObject("BluePlanetOverlay");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            overlaySprite = obj.AddComponent<SpriteRenderer>();
            overlaySprite.sortingOrder = 10; // 确保显示在地面之上
        }
        else
        {
            overlaySprite = overlay.GetComponent<SpriteRenderer>();
        }
        // 显示当前状态的图片
        UpdateOverlaySprite();
    }

    void UpdateOverlaySprite()
    {
        if (overlaySprite == null) return;
        if (isFixed && fixedSprite != null)
            overlaySprite.sprite = fixedSprite;
        else if (brokenSprite != null)
            overlaySprite.sprite = brokenSprite;
    }

    public void Trigger()
    {
        if (isFixed) return;
        GameManager.Instance.SetInEvent(true);
        ShowMainPanel();
    }

    void ShowMainPanel()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "破损得不成样子的留声机");
        float y = -150f;
        CreateButton(panel, "离开", () => { GameManager.Instance.SetInEvent(false); Destroy(panel); }, y);
        y -= 60f;
        CreateButton(panel, "尝试修复（hp-10）", () => {
            GameManager.Instance.TakeDamage(10);
            Destroy(panel);
            ShowAfterFirstRepair();
        }, y);
    }

    void ShowAfterFirstRepair()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "你一顿修，留声机更坏了");
        float y = -150f;
        CreateButton(panel, "放弃离开", () => { GameManager.Instance.SetInEvent(false); Destroy(panel); }, y);
        y -= 60f;
        CreateButton(panel, "再继续修（hp-10）", () => {
            GameManager.Instance.TakeDamage(10);
            Destroy(panel);
            ShowAfterSecondRepair();
        }, y);
    }

    void ShowAfterSecondRepair()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "外形完整了，但就是不响");
        float y = -150f;
        CreateButton(panel, "放弃离开", () => { GameManager.Instance.SetInEvent(false); Destroy(panel); }, y);
        y -= 60f;
        CreateButton(panel, "狠狠一拍（hp-5）", () => {
            GameManager.Instance.TakeDamage(5);
            Destroy(panel);
            FinalFix();
        }, y);
    }

    void FinalFix()
    {
        GameManager.Instance.AddShield(65);
        isFixed = true;
        UpdateOverlaySprite();
        StartCoroutine(ShowSuccessMessage("留声机传出悠扬的歌声\n~泱泱汉水，浩浩长江~\n你感觉浑身充满了力量（护盾+65）"));
    }

    IEnumerator ShowSuccessMessage(string msg)
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

    // ---------- UI 辅助方法 ----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;
        GameObject panel = new GameObject("BluePlanetPanel");
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