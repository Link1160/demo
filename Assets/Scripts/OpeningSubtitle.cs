using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpeningSubtitle : MonoBehaviour
{
    [TextArea] public string[] subtitles;
    public Text subtitleText;
    public GameObject subtitlePanel;
    public GameObject mainUIPanel;          // 将主界面（标题、按钮等）拖入
    public float delayBeforeStart = 1f;
    public string mainSceneName = "SampleScene";

    private int currentIndex = 0;
    private bool canAdvance = false;

    void Start()
    {
        // 确保一开始字幕面板隐藏
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    // 按钮调用的方法
    public void StartSubtitles()
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(false);
        subtitlePanel.SetActive(true);
        subtitleText.text = "";
        Invoke("ShowFirstSubtitle", delayBeforeStart);
    }

    void ShowFirstSubtitle()
    {
        canAdvance = true;
        NextSubtitle();
    }

    void NextSubtitle()
    {
        if (currentIndex < subtitles.Length)
        {
            subtitleText.text = subtitles[currentIndex];
            currentIndex++;
        }
        else
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    void Update()
    {
        if (canAdvance && Input.GetMouseButtonDown(0))
        {
            NextSubtitle();
        }
    }
}
