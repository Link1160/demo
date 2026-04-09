//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class BarracksEvent : MonoBehaviour
//{
//    public int barracksID; // 1,2,3
//    private bool hasJoined = false;   // 该军营是否已参军过
//    private int stepCost = 6;

//    // 军营描述文案
//    private string GetDescription()
//    {
//        switch (barracksID)
//        {
//            case 1: return "听党指挥、能打胜仗、作风优良";
//            case 2: return "请党放心，强国有我！";
//            case 3: return "青春只有一次，参军荣耀一生！";
//            default: return "";
//        }
//    }

//    // 根据全局参军次数获取参军按钮的文案和奖励
//    private (string buttonText, int reward) GetJoinOption()
//    {
//        int count = GameManager.Instance.totalJoinCount;
//        if (count == 0) return ("我要参军！", 3);
//        else if (count == 1) return ("再次向祖国报道！", 6);
//        else if (count == 2) return ("再次奔赴祖国最需要的地方！", 12);
//        else return ("", 0); // 已经参军三次，不应再出现
//    }

//    // 外部调用触发事件
//    public void Trigger()
//    {
//        GameManager.Instance.SetInEvent(true);
//        // 动态生成UI
//        GameObject panel = CreatePanel();
//        ShowDescription(panel);
//        if (!hasJoined && GameManager.Instance.totalJoinCount < 3)
//        {
//            // 显示参军按钮（根据当前总次数）
//            var joinOption = GetJoinOption();
//            CreateButton(panel, joinOption.buttonText, () => OnJoin(panel, joinOption.reward));
//        }
//        // 总是显示离开按钮
//        CreateButton(panel, "离开", () =>
//        {
//            GameManager.Instance.SetInEvent(false);
//            Destroy(panel);
//        });
//    }

//    private GameObject CreatePanel()
//    {
//        Canvas canvas = FindObjectOfType<Canvas>();
//        if (canvas == null)
//        {
//            Debug.LogError("场景中没有 Canvas，请在场景中添加一个 Canvas 对象");
//            return null;
//        }
//        GameObject panel = new GameObject("BarracksPanel");
//        panel.transform.SetParent(canvas.transform, false);
//        Image bg = panel.AddComponent<Image>();
//        bg.color = new Color(0, 0, 0, 0.8f);
//        RectTransform rect = panel.GetComponent<RectTransform>();
//        rect.anchorMin = Vector2.zero;
//        rect.anchorMax = Vector2.one;
//        rect.sizeDelta = Vector2.zero;
//        return panel;
//    }

//    private void ShowDescription(GameObject panel)
//    {
//        Text desc = CreateText(panel.transform, GetDescription(), 24);
//        desc.rectTransform.anchoredPosition = new Vector2(0, 100);
//    }

//    private void CreateButton(GameObject panel, string label, System.Action onClick)
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
//        rect.anchoredPosition = new Vector2(0, -50 - (panel.transform.childCount * 60));
//        btn.onClick.AddListener(() => onClick());
//    }

//    private Text CreateText(Transform parent, string text, int fontSize)
//    {
//        GameObject go = new GameObject("Text");
//        go.transform.SetParent(parent, false);
//        Text txt = go.AddComponent<Text>();
//        txt.text = text;
//        // 使用新字体
//        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//        if (font == null)
//        {
//            // 备用：尝试加载 Arial（某些版本仍可用）
//            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        }
//        if (font == null)
//        {
//            // 最终备用：创建默认字体（可能会报错，但不会中断）
//            font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
//        }
//        txt.font = font;
//        txt.fontSize = fontSize;
//        txt.alignment = TextAnchor.MiddleCenter;
//        txt.color = Color.white;
//        RectTransform rect = go.GetComponent<RectTransform>();
//        rect.sizeDelta = new Vector2(400, 50);
//        rect.anchoredPosition = Vector2.zero;
//        return txt;
//    }

//    private void OnJoin(GameObject panel, int reward)
//    {
//        if (GameManager.Instance.steps < stepCost)
//        {
//            Debug.Log("步数不足");
//            GameManager.Instance.SetInEvent(false);
//            Destroy(panel);
//            return;
//        }
//        GameManager.Instance.ConsumeSteps(stepCost);
//        hasJoined = true;

//        // 获取当前参军次数（增加前）
//        int currentCount = GameManager.Instance.totalJoinCount;
//        string message = "";
//        if (currentCount == 0) message = "卸甲而归，初心不改";
//        else if (currentCount == 1) message = "戎装虽解，军魂犹在";
//        else if (currentCount == 2) message = "本色长存，有召必回";

//        // 增加次数和军魂
//        GameManager.Instance.totalJoinCount++;
//        GameManager.Instance.AddMilitarySpirit(reward);

//        Destroy(panel);
//        StartCoroutine(ShowTransitionAndReward(reward, message));
//    }

//    private IEnumerator ShowTransitionAndReward(int reward, string message)
//    {
//        // 确保事件期间锁定
//        GameManager.Instance.SetInEvent(true);

//        Canvas canvas = FindObjectOfType<Canvas>();
//        GameManager.Instance.SetInEvent(false);
//        if (canvas == null)
//        {
//            Debug.LogError("找不到Canvas，无法显示过渡面板");
//            yield break;
//        }

//        GameObject panel = new GameObject("TransitionPanel");
//        panel.transform.SetParent(canvas.transform, false);
//        Image bg = panel.AddComponent<Image>();
//        bg.color = new Color(0, 0, 0, 0.8f);
//        RectTransform rect = panel.GetComponent<RectTransform>();
//        rect.anchorMin = Vector2.zero;
//        rect.anchorMax = Vector2.one;
//        rect.sizeDelta = Vector2.zero;

//        Text txt = CreateText(panel.transform, "数年后...", 30);
//        txt.rectTransform.anchoredPosition = new Vector2(0, 0);

//        yield return new WaitForSeconds(2f);
//        // 拼接文案和军魂，例如 "卸甲而归，初心不改（军魂+3）"
//        txt.text = $"{message}（军魂+{reward}）";
//        yield return new WaitForSeconds(2f);

//        GameManager.Instance.SetInEvent(false);
//        Destroy(panel);
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BarracksEvent : MonoBehaviour
{
    public int barracksID; // 1,2,3
    private bool hasJoined = false;   // 该军营是否已参军过
    private int stepCost = 6;

    // 军营描述文案
    private string GetDescription()
    {
        switch (barracksID)
        {
            case 1: return "听党指挥、能打胜仗、作风优良";
            case 2: return "请党放心，强国有我！";
            case 3: return "青春只有一次，参军荣耀一生！";
            default: return "";
        }
    }

    // 根据全局参军次数获取参军按钮的文案和奖励
    private (string buttonText, int reward) GetJoinOption()
    {
        int count = GameManager.Instance.totalJoinCount;
        if (count == 0) return ("我要参军！", 10);
        else if (count == 1) return ("再次向祖国报道！", 20);
        else if (count == 2) return ("再次奔赴祖国最需要的地方！", 999);
        else return ("", 0);
    }

    // 外部调用触发事件
    public void Trigger()
    {
        GameManager.Instance.SetInEvent(true);
        GameObject panel = CreatePanel();
        if (panel == null) return;
        ShowDescription(panel);
        if (!hasJoined && GameManager.Instance.totalJoinCount < 3)
        {
            var joinOption = GetJoinOption();
            CreateButton(panel, joinOption.buttonText, () => OnJoin(panel, joinOption.reward));
        }
        CreateButton(panel, "离开", () =>
        {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        });
    }

    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中没有 Canvas");
            return null;
        }
        GameObject panel = new GameObject("BarracksPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;  // 确保阻挡点击
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return panel;
    }

    private void ShowDescription(GameObject panel)
    {
        Text desc = CreateText(panel.transform, GetDescription(), 24);
        desc.rectTransform.anchoredPosition = new Vector2(0, 100);
    }

    private void CreateButton(GameObject panel, string label, System.Action onClick)
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
        rect.anchoredPosition = new Vector2(0, -50 - (panel.transform.childCount * 60));
        btn.onClick.AddListener(() => onClick());
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
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 50);
        rect.anchoredPosition = Vector2.zero;
        return txt;
    }

    private void OnJoin(GameObject panel, int reward)
    {
        if (GameManager.Instance.steps < stepCost)
        {
            Debug.Log("步数不足");
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
            return;
        }
        GameManager.Instance.ConsumeSteps(stepCost);
        hasJoined = true;

        int currentCount = GameManager.Instance.totalJoinCount;
        string message = "";
        if (currentCount == 0) message = "卸甲而归，初心不改";
        else if (currentCount == 1) message = "戎装虽解，军魂犹在";
        else if (currentCount == 2) message = "本色长存，有召必回";

        GameManager.Instance.totalJoinCount++;
        GameManager.Instance.AddShield(reward);

        // 注意：不要在这里解锁，让过渡协程负责
        Destroy(panel);
        StartCoroutine(ShowTransitionAndReward(reward, message));
    }

    private IEnumerator ShowTransitionAndReward(int reward, string message)
    {
        GameManager.Instance.SetInEvent(true);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("找不到Canvas，无法显示过渡面板");
            GameManager.Instance.SetInEvent(false);
            yield break;
        }

        GameObject panel = new GameObject("TransitionPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // 直接创建文本，不依赖 CreateText（因为 CreateText 固定了尺寸）
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panel.transform, false);
        Text txt = textGo.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 30);
        txt.fontSize = 30;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Truncate;

        // 文本区域占满面板，留出边距
        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.1f, 0.1f);
        txtRect.anchorMax = new Vector2(0.9f, 0.9f);
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        txt.text = "数年后...";

        // 等待第一次点击
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return null;

        // 显示奖励文案
        txt.text = $"{message}（护盾+{reward}）";

        // 等待第二次点击
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return null;

        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }
}