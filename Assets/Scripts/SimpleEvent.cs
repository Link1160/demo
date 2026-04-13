//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using static UnityEngine.Rendering.DebugUI;

//public class SimpleEvent : MonoBehaviour
//{
//    public bool becomeWalkableOnComplete = false; // 完成后是否变为可走地面
//    public string eventName;
//    [TextArea] public string description;
//    public string actionButtonText = "进去看看";
//    [TextArea] public string[] successMessages;
//    public int stepCost = 3;
//    public int spiritReward = 3;
//    [TextArea] public string successMessage;   // 成功时显示的文本，如果为空则不显示

//    private bool hasTriggered = false;
//    public int hpChange = 0;   // 正数加血，负数扣血

//    public void Trigger()
//    {
//        GameManager.Instance.SetInEvent(true);
//        if (hasTriggered)
//        {
//            Debug.Log($"{eventName} 已触发过");
//            return;
//        }

//        GameObject panel = CreatePanel();
//        if (panel == null) return;

//        ShowDescription(panel, description);
//        float buttonY = -150f;
//        CreateButton(panel, "离开", () =>{
//            GameManager.Instance.SetInEvent(false);
//            Destroy(panel);
//        }, buttonY);
//        buttonY -= 60f;
//        CreateButton(panel, actionButtonText, () => OnAction(panel), buttonY);
//    }

//    private void OnAction(GameObject panel)
//    {
//        if (GameManager.Instance.steps < stepCost)
//        {
//            Debug.Log("步数不足");
//            GameManager.Instance.SetInEvent(false);
//            Destroy(panel);
//            return;
//        }

//        // 扣步数、加属性
//        GameManager.Instance.ConsumeSteps(stepCost);
//        GameManager.Instance.AddSpirit(spiritReward);
//        if (hpChange != 0) GameManager.Instance.ChangeHealth(hpChange);
//        hasTriggered = true;

//        // 如果需要，将格子变为可走
//        if (becomeWalkableOnComplete)
//        {
//            Tile tile = GetComponent<Tile>();
//            if (tile != null) tile.type = TileType.Ground;
//        }

//        // 销毁原事件面板
//        Destroy(panel);

//        // 显示成功消息（如果有）
//        if (!string.IsNullOrEmpty(successMessage))
//        {
//            StartCoroutine(ShowSuccessMessage(successMessage));
//        }
//        else
//        {
//            GameManager.Instance.SetInEvent(false);
//        }
//    }

//    private IEnumerator ShowSuccessMessage(string msg)
//    {
//        GameManager.Instance.SetInEvent(true);

//        // 查找 Canvas
//        Canvas canvas = FindObjectOfType<Canvas>();
//        if (canvas == null)
//        {
//            Debug.LogError("没有 Canvas，无法显示消息");
//            GameManager.Instance.SetInEvent(false);
//            yield break;
//        }

//        // 创建面板
//        GameObject panel = new GameObject("SuccessMsg");
//        panel.transform.SetParent(canvas.transform, false);
//        panel.AddComponent<CanvasRenderer>();
//        Image bg = panel.AddComponent<Image>();
//        bg.color = new Color(0, 0, 0, 0.8f);
//        bg.raycastTarget = true;
//        RectTransform rect = panel.GetComponent<RectTransform>();
//        rect.anchorMin = Vector2.zero;
//        rect.anchorMax = Vector2.one;
//        rect.sizeDelta = Vector2.zero;

//        // 创建文本（不用 CreateText，直接做）
//        GameObject textGo = new GameObject("Text");
//        textGo.transform.SetParent(panel.transform, false);
//        Text txt = textGo.AddComponent<Text>();
//        txt.text = msg;
//        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
//        txt.fontSize = 24;
//        txt.alignment = TextAnchor.MiddleCenter;
//        txt.color = Color.white;
//        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
//        RectTransform txtRect = txt.GetComponent<RectTransform>();
//        txtRect.anchorMin = new Vector2(0.1f, 0.1f);
//        txtRect.anchorMax = new Vector2(0.9f, 0.9f);
//        txtRect.offsetMin = Vector2.zero;
//        txtRect.offsetMax = Vector2.zero;

//        // 等待玩家点击
//        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

//        GameManager.Instance.SetInEvent(false);
//        Destroy(panel);
//    }

//    // ---------- UI 辅助方法 ----------
//    private GameObject CreatePanel()
//    {
//        Canvas canvas = FindObjectOfType<Canvas>();
//        if (canvas == null)
//        {
//            Debug.LogError("找不到 Canvas");
//            return null;
//        }
//        GameObject panel = new GameObject("EventPanel");
//        panel.transform.SetParent(canvas.transform, false);
//        Image bg = panel.AddComponent<Image>();
//        bg.color = new Color(0, 0, 0, 0.8f);
//        RectTransform rect = panel.GetComponent<RectTransform>();
//        rect.anchorMin = Vector2.zero;
//        rect.anchorMax = Vector2.one;
//        rect.sizeDelta = Vector2.zero;
//        return panel;
//    }

//    private void ShowDescription(GameObject panel, string desc)
//    {
//        GameObject textGo = new GameObject("DescriptionText");
//        textGo.transform.SetParent(panel.transform, false);
//        Text txt = textGo.AddComponent<Text>();
//        txt.text = desc;
//        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 24);
//        txt.font = font;
//        txt.fontSize = 24;
//        txt.alignment = TextAnchor.MiddleCenter;
//        txt.color = Color.white;
//        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
//        txt.verticalOverflow = VerticalWrapMode.Truncate;

//        RectTransform textRect = txt.GetComponent<RectTransform>();
//        // 设置文本区域占面板中间大部分
//        textRect.anchorMin = new Vector2(0.1f, 0.3f);
//        textRect.anchorMax = new Vector2(0.9f, 0.7f);
//        textRect.offsetMin = Vector2.zero;
//        textRect.offsetMax = Vector2.zero;
//    }

//    private void CreateButton(GameObject panel, string label, UnityEngine.Events.UnityAction onClick, float yPos)
//    {
//        GameObject go = new GameObject("Button");
//        go.transform.SetParent(panel.transform, false);
//        Button btn = go.AddComponent<Button>();
//        Image img = go.AddComponent<Image>();
//        img.color = Color.gray;
//        Text txt = CreateText(go.transform, label, 20);
//        txt.rectTransform.anchoredPosition = Vector2.zero;
//        RectTransform rect = go.GetComponent<RectTransform>();
//        rect.sizeDelta = new Vector2(200, 50);
//        rect.anchoredPosition = new Vector2(0, yPos);
//        btn.onClick.AddListener(onClick);
//    }

//    private Text CreateText(Transform parent, string text, int fontSize)
//    {
//        // 替代 CreateText 的简单实现
//        GameObject textGo = new GameObject("Message");
//        textGo.transform.SetParent(panel.transform, false);
//        Text txt = textGo.AddComponent<Text>();
//        txt.text = msg;
//        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 24);
//        txt.font = font;
//        txt.fontSize = 24;
//        txt.alignment = TextAnchor.MiddleCenter;
//        txt.color = Color.white;
//        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
//        RectTransform txtRect = txt.GetComponent<RectTransform>();
//        txtRect.anchorMin = new Vector2(0.1f, 0.1f);
//        txtRect.anchorMax = new Vector2(0.9f, 0.9f);
//        txtRect.offsetMin = Vector2.zero;
//        txtRect.offsetMax = Vector2.zero;
//    }
//}

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

        GameManager.Instance.ConsumeSteps(stepCost);
        GameManager.Instance.AddShield(spiritReward);
        if (hpChange != 0)
        {
            if (hpChange > 0)
                GameManager.Instance.Heal(hpChange);
            else
                GameManager.Instance.TakeDamage(-hpChange);
        }

        hasTriggered = true;

        if (successMessages != null && successMessages.Length > 0)
        {
            finalMessage = successMessages[successMessages.Length - 1]; // 只取最后一段
        }

        if (becomeWalkableOnComplete)
        {
            Tile tile = GetComponent<Tile>();
            if (tile != null) tile.type = TileType.Ground;
        }

        Destroy(panel);

        // 显示成功消息（支持多段）
        if (successMessages != null && successMessages.Length > 0)
        {
            StartCoroutine(ShowSuccessMessages(successMessages));
        }
        else
        {
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