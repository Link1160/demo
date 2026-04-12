using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToStart : MonoBehaviour
{
    public void GoToStart()
    {
        // 直接加载开始场景，所有管理器都会被重新创建，状态自动重置
        SceneManager.LoadScene("StartScene");
    }
}
