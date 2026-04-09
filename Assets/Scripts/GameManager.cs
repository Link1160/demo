using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
                // 可以触发游戏结束逻辑（例如加载失败界面）
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
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁
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
