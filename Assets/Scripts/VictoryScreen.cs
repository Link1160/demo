//using UnityEngine;
//using UnityEngine.UI;

//public class VictoryScreen : MonoBehaviour
//{
//    public Text scoreText;

//    void Start()
//    {
//        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
//        if (scoreText != null)
//            scoreText.text = "Score:" + finalScore;
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    public Text scoreText;

    void Start()
    {
        Debug.Log("VictoryScreen Start 执行");
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        Debug.Log("读取到的分数: " + finalScore);
        if (scoreText != null)
        {
            scoreText.text = "最终分数：" + finalScore;
            Debug.Log("分数文本已设置: " + scoreText.text);
        }
        else
        {
            Debug.LogError("scoreText 未赋值！");
        }
    }
}