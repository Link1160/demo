using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiStepEvent : MonoBehaviour
{
    public string eventName;
    [TextArea] public string initialDescription;
    public int initialStepCost = 0;
    public EventStep[] steps;
    public int increaseMaxHealth;

    private bool hasTriggered = false;
    private int currentStepIndex = 0;
    private GameObject currentPanel;

    private string finalMessage; // 存储事件完全结束时的成功消息

    public Sprite evSprite;
    public Sprite[] evSprites;   // 多个电动车图片（用于随机）
    public bool randomEVSprite = true; // 是否随机，false 则使用 evSprite 固定图片
    public Vector2Int[] evSpawnPositions; // 在 Inspector 中填入五个坐标

    public bool checkDirection = false; // 仅生活门事件勾选
    public Vector2Int doorRightPos;          // 门右侧的格子坐标（用于b选项移动）
    public Vector2Int signSpawnPos;          // 木牌生成坐标（用于c选项）
    public string signMessage = "上面写到：东区大门"; // 木牌文案
    public Sprite signSprite;   // 木牌图片

    private void SpawnEVSpots()
    {
        foreach (Vector2Int pos in evSpawnPositions)
        {
            Tile tile = MapManager.Instance.GetTile(pos);
            if (tile == null) continue;

            // 1. 将格子设为墙（不可走）
            tile.type = TileType.Wall;

            // 2. 移除该格子上可能残留的事件组件
            SimpleEvent se = tile.GetComponent<SimpleEvent>();
            if (se != null) DestroyImmediate(se);
            BarracksEvent be = tile.GetComponent<BarracksEvent>();
            if (be != null) DestroyImmediate(be);
            MultiStepEvent me = tile.GetComponent<MultiStepEvent>();
            if (me != null) DestroyImmediate(me);

            // 3. 创建电动车图片的子物体（叠加在地面之上）
            GameObject evObj = new GameObject("EV_" + pos.x + "_" + pos.y);
            evObj.transform.SetParent(tile.transform);
            evObj.transform.localPosition = Vector3.zero; // 与地面格子重合
            evObj.transform.localScale = Vector3.one;

            SpriteRenderer sr = evObj.AddComponent<SpriteRenderer>();
            // 选择图片（随机或固定）
            if (randomEVSprite && evSprites != null && evSprites.Length > 0)
            {
                int idx = Random.Range(0, evSprites.Length);
                sr.sprite = evSprites[idx];
            }
            else if (evSprite != null)
            {
                sr.sprite = evSprite;
            }
            else
            {
                Debug.LogWarning($"电动车格子 {pos} 没有配置图片");
            }

            // 设置渲染层级，确保电动车显示在地面之上
            sr.sortingLayerName = "Foreground"; // 需要确保有该层，或者用较高 Order
            sr.sortingOrder = 10;
        }
    }

    [System.Serializable]
    public class EventStep
    {
        [TextArea] public string description;
        public Option[] options;
    }

    [System.Serializable]
    public class Option
    {
        public bool endsEvent = true;   // 默认为 true，表示选择后事件结束（不再触发）
        public string buttonText;
        public int stepCost;
        public int spiritReward;
        [TextArea] public string successMessage;
        public int nextStepIndex = -1;
        public int increaseMaxHealth;
        public int increaseHealth;
        public int hpChange; // 正数为加血，负数为扣血
        public int addShield;
        public bool becomeWalkableOnComplete; // 仅当该选项结束事件时生效

        public bool moveToDoorRight;   // 是否移动到门右侧
        public bool spawnSign;         // 是否生成木牌

    }

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

        // 仅当 checkDirection 为 true 时才进行方向判断（生活门专用）
        if (checkDirection)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                // 确保 player 的 gridPos 已初始化（非默认值）
                if (player.gridPos != Vector2Int.zero)
                {
                    Vector2Int myPos = GetComponent<Tile>().gridPos;
                    if (player.gridPos.x > myPos.x) // 从右侧触发
                    {
                        GameManager.Instance.SetInEvent(true);
                        GameObject panel = CreatePanel();
                        ShowDescription(panel, "门不能从这一侧打开");
                        CreateButton(panel, "确定", () => {
                            GameManager.Instance.SetInEvent(false);
                            Destroy(panel);
                        }, -150f);
                        return;
                    }
                }
            }
        }

        // 正常触发逻辑（锁定地图）
        GameManager.Instance.SetInEvent(true);

        // 如果有保存的步骤索引，则从该步骤开始
        if (currentStepIndex >= 0 && currentStepIndex < steps.Length)
        {
            var step = steps[currentStepIndex];
            ShowStep(step.description, step.options, 0);
        }
        else
        {
            currentStepIndex = 0;
            ShowStep(initialDescription, steps[0].options, initialStepCost);
        }
    }

    private void ShowStep(string desc, Option[] opts, int cost)
    {
        if (currentPanel != null)
        {
            //GameManager.Instance.SetInEvent(false);
            Destroy(currentPanel);
        }
        currentPanel = CreatePanel();
        if (currentPanel == null) return;

        ShowDescription(currentPanel, desc);
        float y = -150f;
        foreach (var opt in opts)
        {
            CreateButton(currentPanel, opt.buttonText, () => OnOptionSelected(opt), y);
            y -= 60f;
        }
    }

    private void OnOptionSelected(Option opt)
    {
        if (opt.increaseMaxHealth != 0)
        {
            GameManager.Instance.IncreaseMaxHealth(opt.increaseMaxHealth);
        }
        if (opt.increaseHealth != 0)
        {
            GameManager.Instance.Heal(opt.increaseHealth);
            //GameManager.Instance.health += opt.increaseHealth;
            //if (GameManager.Instance.health > GameManager.Instance.maxHealth)
            //    GameManager.Instance.health = GameManager.Instance.maxHealth;
        }
        if (opt.addShield != 0)
        {
            GameManager.Instance.AddShield(opt.addShield);
        }

        if (opt.buttonText == "关掉它" && evSpawnPositions != null && evSpawnPositions.Length > 0)
        {
            SpawnEVSpots();
        }
        if (GameManager.Instance.steps < opt.stepCost)
        {
            Debug.Log("步数不足");
            GameManager.Instance.SetInEvent(false);
            Destroy(currentPanel);
            return;
        }
        GameManager.Instance.ConsumeSteps(opt.stepCost);
        if (opt.spiritReward != 0)
            GameManager.Instance.AddShield(opt.spiritReward);

        if (opt.hpChange != 0)
        {
            if (opt.hpChange > 0)
                GameManager.Instance.Heal(opt.hpChange);   // 正数代表加血
            else
                GameManager.Instance.TakeDamage(-opt.hpChange); // 负数代表扣血，取绝对值传入
        }

        if (opt.nextStepIndex == -1)
        {
            if (opt.endsEvent)
            {
                hasTriggered = true;
                finalMessage = opt.successMessage; // 保存消息
                if (opt.becomeWalkableOnComplete)
                {
                    Tile tile = GetComponent<Tile>();
                    if (tile != null) tile.type = TileType.Ground;
                }
                GameManager.Instance.SetInEvent(false);
                Destroy(currentPanel);
                if (!string.IsNullOrEmpty(opt.successMessage))
                    StartCoroutine(ShowSuccessMessage(opt.successMessage));
            }
            else
            {
                // 事件未结束，只关闭当前面板，不设置 hasTriggered
                GameManager.Instance.SetInEvent(false);
                Destroy(currentPanel);
                if (!string.IsNullOrEmpty(opt.successMessage))
                    StartCoroutine(ShowSuccessMessage(opt.successMessage));
                // 注意：下次触发时，仍从初始步骤开始，因为 hasTriggered 仍为 false
            }
        }
        else
        {
            currentStepIndex = opt.nextStepIndex;
            var nextStep = steps[currentStepIndex];
            ShowStep(nextStep.description, nextStep.options, 0);
        }

        if (opt.moveToDoorRight)
        {
            // 移动玩家到门右侧格子
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.MoveTo(doorRightPos);
            }
        }

        if (opt.spawnSign)
        {
            SpawnSign();
        }

    }

    //private void SpawnSign()
    //{
    //    Tile tile = MapManager.Instance.GetTile(signSpawnPos);
    //    if (tile == null) return;

    //    // 设为不可走（墙）
    //    tile.type = TileType.Event;

    //    // 移除可能残留的事件组件
    //    SimpleEvent se = tile.GetComponent<SimpleEvent>();
    //    if (se != null) DestroyImmediate(se);

    //    // 添加新的 SimpleEvent 组件
    //    se = tile.gameObject.AddComponent<SimpleEvent>();
    //    se.eventName = "东区大门";
    //    se.description = signMessage;
    //    se.actionButtonText = "";   // 无动作按钮，只显示描述和离开
    //    se.successMessages = new string[0];
    //    se.stepCost = 0;
    //    se.spiritReward = 0;
    //    se.hpChange = 0;
    //    se.becomeWalkableOnComplete = false;
    //    // hasTriggered 默认为 false，可重复触发
    //}

    private void SpawnSign()
    {
        Tile tile = MapManager.Instance.GetTile(signSpawnPos);
        if (tile == null) return;

        // 1. 确保格子类型为 Event（不可走，但可触发事件）
        tile.type = TileType.Event;

        // 2. 移除原有事件组件，添加新的 SimpleEvent
        SimpleEvent se = tile.GetComponent<SimpleEvent>();
        if (se != null) DestroyImmediate(se);
        se = tile.gameObject.AddComponent<SimpleEvent>();
        se.eventName = "东区大门";
        se.description = signMessage;
        se.actionButtonText = "";   // 只显示描述和离开按钮
        se.successMessages = new string[0];
        se.stepCost = 0;
        se.spiritReward = 0;
        se.hpChange = 0;
        se.becomeWalkableOnComplete = false;

        // 3. 创建木牌子物体（显示图片）
        GameObject signObj = new GameObject("Sign_" + signSpawnPos.x + "_" + signSpawnPos.y);
        signObj.transform.SetParent(tile.transform);
        signObj.transform.localPosition = Vector3.zero;
        signObj.transform.localScale = Vector3.one;

        SpriteRenderer sr = signObj.AddComponent<SpriteRenderer>();
        if (signSprite != null)
        {
            sr.sprite = signSprite;
        }
        else
        {
            Debug.LogWarning($"木牌格子 {signSpawnPos} 没有配置图片");
        }
        // 设置渲染层级，确保显示在地面之上
        sr.sortingLayerName = "Foreground";  // 需要确保该层存在，或使用较高 sortingOrder
        sr.sortingOrder = 10;
    }

    // ---------- UI 辅助 ----------
    private GameObject CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("找不到 Canvas"); return null; }
        GameObject panel = new GameObject("EventPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return panel;
    }

    private void ShowDescription(GameObject panel, string desc)
    {
        GameObject go = new GameObject("Description");
        go.transform.SetParent(panel.transform, false);
        Text txt = go.AddComponent<Text>();
        txt.text = desc;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

    //private IEnumerator ShowSuccessMessage(string msg)
    //{
    //    GameManager.Instance.SetInEvent(true);
    //    Canvas canvas = FindObjectOfType<Canvas>();
    //    if (canvas == null) yield break;
    //    GameObject panel = new GameObject("SuccessPanel");
    //    panel.transform.SetParent(canvas.transform, false);
    //    Image bg = panel.AddComponent<Image>();
    //    bg.color = new Color(0, 0, 0, 0.8f);
    //    RectTransform rect = panel.GetComponent<RectTransform>();
    //    rect.anchorMin = Vector2.zero;
    //    rect.anchorMax = Vector2.one;
    //    rect.sizeDelta = Vector2.zero;

    //    Text txt = CreateText(panel.transform, msg, 24);
    //    txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
    //    txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
    //    txt.rectTransform.offsetMin = Vector2.zero;
    //    txt.rectTransform.offsetMax = Vector2.zero;
    //    txt.horizontalOverflow = HorizontalWrapMode.Wrap;

    //    yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
    //    GameManager.Instance.SetInEvent(false);
    //    Destroy(panel);
    //}

    private IEnumerator ShowSuccessMessage(string msg)
    {
        GameManager.Instance.SetInEvent(true);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("找不到Canvas");
            GameManager.Instance.SetInEvent(false);
            yield break;
        }

        GameObject panel = new GameObject("SuccessMsg");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        bg.raycastTarget = true;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Text txt = CreateText(panel.transform, msg, 24);
        txt.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        txt.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.alignment = TextAnchor.MiddleCenter;

        // 关键：面板创建并显示完成后，等待一帧，确保鼠标按下事件不会立即触发
        yield return null;

        // 等待玩家点击
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        yield return null;

        GameManager.Instance.SetInEvent(false);
        Destroy(panel);
    }
}
