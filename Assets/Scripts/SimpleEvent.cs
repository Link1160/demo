using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleEvent : MonoBehaviour
{
    [Header("事件配置")]
    public string eventName;
    [TextArea] public string description;
    public string actionButtonText = "进去看看";
    [TextArea] public string[] successMessages;   // 多段成功消息
    public int stepCost = 3;
    public int spiritReward = 3;
    public int hpChange = 0;
    public bool becomeWalkableOnComplete = false;

    private bool hasTriggered = false;
    private string finalMessage; // 事件完全结束后的成功消息

    public void Trigger()
    {
        if (hasTriggered)
        {
            if (!string.IsNullOrEmpty(finalMessage))
            {
                GameManager.Instance.SetInEvent(true);
                StartCoroutine(ShowSuccessMessage(finalMessage));
            }
            return;
        }

        GameManager.Instance.SetInEvent(true);

        GameObject panel = CreatePanel();
        if (panel == null) return;

        ShowDescription(panel, description);
        float buttonY = -150f;
        CreateButton(panel, "离开", () =>
        {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, buttonY);
        if (!string.IsNullOrEmpty(actionButtonText))
        {
            buttonY -= 60f;
            CreateButton(panel, actionButtonText, () => OnAction(panel), buttonY);
        }
    }

    private void OnAction(GameObject panel)
    {
        if (GameManager.Instance.steps < stepCost)
        {
            Debug.Log("步数不足");
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
            return;
        }

        // 保存最终消息（取数组最后一条，用于下次点击显示）
        if (successMessages != null && successMessages.Length > 0)
        {
            finalMessage = successMessages[successMessages.Length - 1];
        }
        else
        {
            finalMessage = ""; // 或者不保存
        }

        Destroy(panel);

        // 显示成功消息（支持多段）
        if (successMessages != null && successMessages.Length > 0)
        {
            StartCoroutine(ShowSuccessMessages(successMessages));
        }
        else
        {
            ApplyEffects();
            GameManager.Instance.SetInEvent(false);
        }
    }

    private IEnumerator ShowSuccessMessages(string[] messages)
    {
        GameManager.Instance.SetInEvent(true);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("没有 Canvas");
            GameManager.Instance.SetInEvent(false);
            yield break;
        }

        GameObject panel = new GameObject("SuccessMsg");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<CanvasRenderer>();
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // 创建文本组件
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panel.transform, false);
        Text txt = textGo.AddComponent<Text>();
        //txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = GameManager.Instance.chineseFont;   // 新的一行
        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.1f, 0.1f);
        txtRect.anchorMax = new Vector2(0.9f, 0.9f);
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        // 依次显示每段消息
        for (int i = 0; i < messages.Length; i++)
        {
            txt.text = messages[i];
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield return null; // 避免同一帧多次点击
        }

        // 最后一段点击后，执行效果
        ApplyEffects();

        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }

    private IEnumerator ShowSuccessMessage(string msg)
    {
        GameManager.Instance.SetInEvent(true);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("没有 Canvas");
            GameManager.Instance.SetInEvent(false);
            yield break;
        }

        GameObject panel = new GameObject("SuccessMsg");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<CanvasRenderer>();
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panel.transform, false);
        Text txt = textGo.AddComponent<Text>();
        txt.text = msg;
        //txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = GameManager.Instance.chineseFont;   // 新的一行
        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.1f, 0.1f);
        txtRect.anchorMax = new Vector2(0.9f, 0.9f);
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        yield return null;

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }

    private void ApplyEffects()
    {
        // 消耗步数
        GameManager.Instance.ConsumeSteps(stepCost);
        // 加护盾
        if (spiritReward != 0)
            GameManager.Instance.AddShield(spiritReward);
        // 加/减血
        if (hpChange != 0)
        {
            if (hpChange > 0)
                GameManager.Instance.Heal(hpChange);
            else
                GameManager.Instance.TakeDamage(-hpChange);
        }
        // 标记事件已触发
        hasTriggered = true;
        // 如果需要，将格子变为可走
        if (becomeWalkableOnComplete)
        {
            Tile tile = GetComponent<Tile>();
            if (tile != null) tile.type = TileType.Ground;
        }
    }

    // ---------- UI 辅助方法 ----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("找不到 Canvas");
            return null;
        }
        GameObject panel = new GameObject("EventPanel");
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
        //txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = GameManager.Instance.chineseFont;   // 新的一行
        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
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
        UnityEngine.UI.Button btn = go.AddComponent<UnityEngine.UI.Button>();

        Image img = go.AddComponent<Image>();
        img.color = Color.gray;

        //Image img = go.AddComponent<Image>();
        //if (GameManager.Instance != null && GameManager.Instance.defaultButtonSprite != null)
        //{
        //    img.sprite = GameManager.Instance.defaultButtonSprite;
        //    img.color = Color.white;
        //}
        //else
        //{
        //    img.color = Color.gray; // 保底
        //}

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        Text txt = textGo.AddComponent<Text>();
        txt.text = label;
        //txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = GameManager.Instance.chineseFont;   // 新的一行
        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 20);
        txt.fontSize = 20;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = new Vector2(0, yPos);
        btn.onClick.AddListener(onClick);
    }
}