using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    public Text scoreText;   // 注意：如果用普通 Text，类型写 Text；如果用 TMP，写 TMP_Text

    void Start()
    {
        // 从 GameManager 获取分数（假设 GameManager 存在且数值正确）
        int finalScore = GameManager.Instance.health + GameManager.Instance.shield;
        scoreText.text = "最终分数：" + finalScore;
    }
}