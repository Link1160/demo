using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TMP_Text healthText;
    public TMP_Text shieldText;

    public static GameManager Instance;

    public int steps = 100;          // 当前步数
    public int maxHealth = 100;
    public int health = 100;
    public int increaseHealth;
    public int shield = 0;   // 护盾值
    private bool isInEvent = false;
    public bool IsInEvent => isInEvent;

    public bool isInCampusRun = false;   // 是否正在校园跑中
    public bool hasBike = false;         // 是否骑了自行车

    public bool halfDamageMode = false;
    private float accumulatedHalfDamage = 0f;
    private float accumulatedDamage = 0f;   // 用于半伤模式累积伤害

    public Vector2Int endPointPos;   // 终点坐标，在 Inspector 中手动填写
    public TMP_Text distanceText;        // 显示距离的 UI 文本（拖入）
    public void EnableHalfDamage()
    {
        halfDamageMode = true;
        accumulatedHalfDamage = 0f;
    }

    public void WinGame()
    {
        // 计算分数 = 当前血量 + 护盾值
        int finalScore = health + shield;
        Debug.Log($"胜利，分数：{finalScore} (HP={health}, 护盾={shield})");
        // 保存分数到静态变量或 PlayerPrefs，供胜利场景读取
        PlayerPrefs.SetInt("FinalScore", finalScore);
        PlayerPrefs.Save();
        // 加载胜利场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
    }

    //public void LoseGame()
    //{
    //    UnityEngine.SceneManagement.SceneManager.LoadScene("DefeatScene");
    //}
    //public void LoseGame()
    //{
    //    // 停止玩家移动
    //    PlayerController player = FindObjectOfType<PlayerController>();
    //    if (player != null)
    //        player.ForceStopMoving();

    //    // 加载失败场景
    //    SceneManager.LoadScene("DefeatScene");
    //}
    public void LoseGame()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.ForceStopMoving();

        SceneManager.LoadScene("DefeatScene");
    }

    public void ApplyHalfDamage()
    {
        if (!halfDamageMode) return;
        accumulatedHalfDamage += 0.5f;
        int intDamage = Mathf.FloorToInt(accumulatedHalfDamage);
        if (intDamage > 0)
        {
            accumulatedHalfDamage -= intDamage;
            TakeDamage(intDamage);
        }
    }

    // 修改 TakeDamage 方法，支持浮点累积（可选，但为了半伤模式，我们新增一个方法）
    public void TakeDamageHalf(int baseDamage)
    {
        if (halfDamageMode)
        {
            accumulatedDamage += baseDamage * 0.5f;
            int intDamage = Mathf.FloorToInt(accumulatedDamage);
            if (intDamage > 0)
            {
                accumulatedDamage -= intDamage;
                TakeDamage(intDamage);
            }
        }
        else
        {
            TakeDamage(baseDamage);
        }
    }

    public void SetInEvent(bool value)
    {
        isInEvent = value;
    }

    // 统一的伤害处理：先扣护盾，再扣血
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int remainingDamage = damage;
        if (shield > 0)
        {
            int shieldReduce = Mathf.Min(shield, remainingDamage);
            shield -= shieldReduce;
            remainingDamage -= shieldReduce;
            Debug.Log($"护盾抵消 {shieldReduce} 点伤害，剩余护盾 {shield}");
        }

        if (remainingDamage > 0)
        {
            health -= remainingDamage;
            Debug.Log($"受到 {remainingDamage} 点伤害，剩余血量 {health}");
            if (health <= 0)
            {
                health = 0;
                Debug.Log("游戏失败");
                LoseGame();
            }
        }

        UpdateUI(); // 更新显示
    }
    // 加血（不超过最大血量）
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health += amount;
        if (health > maxHealth) health = maxHealth;
        Debug.Log($"恢复 {amount} 点生命，当前血量 {health}");
        UpdateUI();
    }
    // 增加最大血量（同时增加等量当前血量）
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        health += amount;
        Debug.Log($"最大血量增加 {amount}，当前 {maxHealth}/{health}");
        UpdateUI();
        Debug.Log("IncreaseMaxHealth called, healthText is " + (healthText != null ? "assigned" : "null"));
    }
    // 增加护盾
    public void AddShield(int amount)
    {
        if (amount <= 0) return;
        shield += amount;
        Debug.Log($"护盾增加 {amount}，当前护盾 {shield}");
        UpdateUI();
    }
    // 更新UI（你需要在场景中创建对应文本）
    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"{health}/{maxHealth}";
        else
            Debug.LogWarning("healthText 未赋值");

        if (shieldText != null)
            shieldText.text = $"Shield:{shield}";
        else
            Debug.LogWarning("shieldText 未赋值");
    }
    public void UpdateDistanceDisplay(Vector2Int playerPos)
    {
        if (distanceText != null)
        {
            int distance = Mathf.Abs(playerPos.x - endPointPos.x);
            distanceText.text = $"final:{distance} ";
        }
        else
        {
            Debug.LogWarning("distanceText 未赋值");
        }
    }

    public void ChangeHealth(int delta)
    {
        health += delta;
        Debug.Log($"血量变化 {delta}，当前 {health}");
        if (health <= 0)
        {
            Debug.Log("游戏结束");
            // 可以触发失败场景或返回开始界面
        }
    }

    void Start()
    {
        // 确保初始值
        health = maxHealth;
        shield = 0;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int totalJoinCount = 0;      // 已参军次数（0-3）
    public int militarySpirit = 0;      // 军魂值
    public int spirit = 0;   // 精神值，对应策划案中的 ***

    public void AddMilitarySpirit(int value)
    {
        AddShield(value);
    }

    public void AddSpirit(int value)
    {
        AddShield(value);
    }

    public void ConsumeSteps(int amount)
    {
        // 实现步数消耗
    }
}
