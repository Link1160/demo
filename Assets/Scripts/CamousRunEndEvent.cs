using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CampusRunEndEvent : MonoBehaviour
{
    private bool hasFinished = false;
    private bool hasRewardGiven = false;
    public GameObject finishLineObject; // 终点线贴图对象

    public void Trigger()
    {
        if (hasFinished) return;
        GameManager.Instance.SetInEvent(true);

        if (!GameManager.Instance.isInCampusRun)
        {
            ShowNotStarted();
        }
        else
        {
            ShowFinish();
        }
    }

    private void ShowNotStarted()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "未被选择的终点线，一边什么也没有，另一边也什么也没有");
        float y = -150f;
        CreateButton(panel, "离开", () => {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
        y -= 60f;
        CreateButton(panel, "尝试冲线", () => {
            if (!hasRewardGiven)
            {
                GameManager.Instance.AddShield(10);
                hasRewardGiven = true;
            }
            Destroy(panel);
            StartCoroutine(ShowLongMessage("理智的声音制止了你\n你向终点线极目望去，那是属于他人的终点。\n你轻声叹息，或许它曾能够是你的终点，但你选择了脚下的这条路，从此步不停，路不止。\n终点一直存在，但你再无法遇见。"));
        }, y);
    }

    private void ShowFinish()
    {
        GameObject panel = CreatePanel();
        ShowDescription(panel, "终...终于要跑完了");
        float y = -150f;
        CreateButton(panel, "就是不冲线", () => {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        }, y);
        y -= 60f;
        CreateButton(panel, "冲线，结束校园跑", () => {
            string rewardMsg = "";
            if (GameManager.Instance.hasBike)
            {
                GameManager.Instance.AddShield(10);
                rewardMsg = "轻轻松松，游刃有余（护盾+10）";
            }
            else
            {
                GameManager.Instance.EnableHalfDamage();
                rewardMsg = "你感觉身体更结实了（接下来20步每步额外扣0.5血）";
            }
            GameManager.Instance.isInCampusRun = false;
            GameManager.Instance.hasBike = false;
            if (finishLineObject != null) Destroy(finishLineObject);
            Tile tile = GetComponent<Tile>();
            if (tile != null) tile.type = TileType.Ground;
            Destroy(this);
            RemoveStartEvent();
            hasFinished = true;
            // 不要在这里解锁！
            Destroy(panel); // 销毁选项面板
            ShowMessageWithButton(rewardMsg); // 显示成功消息面板（带确定按钮）
        }, y);
    }

    private IEnumerator ShowMessage(string msg)
    {
        // 锁定地图
        GameManager.Instance.SetInEvent(true);

        // 创建面板
        GameObject panel = CreatePanel();
        Text txt = CreateText(panel.transform, msg, 24);
        txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 等待一帧，确保面板已显示
        yield return null;

        // 等待玩家点击
        while (!Input.GetMouseButtonDown(0))
            yield return null;

        // 再次等待一帧，避免同一帧内重复触发
        yield return null;

        // 解锁并销毁
        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }
    private void RemoveStartEvent()
    {
        CampusRunEvent startEvent = FindObjectOfType<CampusRunEvent>();
        if (startEvent != null)
        {
            Destroy(startEvent);
        }
    }

    private IEnumerator ShowLongMessage(string msg)
    {
        GameObject panel = CreatePanel();
        Text txt = CreateText(panel.transform, msg, 24);
        txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }

    private void ShowMessageWithButton(string msg)
    {
        // 创建面板
        GameObject panel = CreatePanel();
        Text txt = CreateText(panel.transform, msg, 24);
        txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 创建确定按钮
        GameObject btnGo = new GameObject("OKButton");
        btnGo.transform.SetParent(panel.transform, false);
        Button btn = btnGo.AddComponent<Button>();
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = Color.gray;
        Text btnText = CreateText(btnGo.transform, "确定", 20);
        btnText.rectTransform.anchorMin = Vector2.zero;
        btnText.rectTransform.anchorMax = Vector2.one;
        btnText.rectTransform.offsetMin = Vector2.zero;
        btnText.rectTransform.offsetMax = Vector2.zero;
        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);
        btnRect.anchoredPosition = new Vector2(0, -150);

        // 按钮点击时解锁并销毁面板
        btn.onClick.AddListener(() => {
            GameManager.Instance.SetInEvent(false);
            Destroy(panel);
        });
    }

    // ---------- UI 辅助方法（同 CampusRunEvent）----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;
        GameObject panel = new GameObject("EndPanel");
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
