using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI 面板設定")]
    public GameObject[] menuPanels;
    public GameObject defaultPanel; // 這是您按下 E 會開啟的那個面板 (背包)

    private bool isMenuOpen = false;

    // 記錄是否已經教過
    private bool hasShownTutorial = false;

    void Start()
    {
        HideAllPanels();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMenuOpen)
            {
                HideAllPanels();
            }
            else
            {
                ShowDefaultPanel();
            }
        }
    }

    // 顯示預設的 Panel (背包)
    public void ShowDefaultPanel()
    {
        HideAllPanels();

        if (defaultPanel != null)
        {
            defaultPanel.SetActive(true);
            isMenuOpen = true;

            // 觸發教學邏輯
            // 如果還沒教過，就呼叫 TutorialManager
            if (!hasShownTutorial)
            {
                if (TutorialManager.Instance != null)
                {
                    Debug.Log("第一次打開背包，觸發教學！");
                    hasShownTutorial = true; // 標記為已教過
                    TutorialManager.Instance.OpenTutorial();
                }
            }
        }
    }

    // 切換面板
    public void SwitchToPanel(GameObject panelToShow)
    {
        HideAllPanels();
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            isMenuOpen = true;
        }
    }

    // 關閉所有介面
    public void HideAllPanels()
    {
        foreach (GameObject panel in menuPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        isMenuOpen = false;
    }
}