using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI 面板設定")]
    public GameObject[] menuPanels;
    public GameObject defaultPanel; // 這是您按下 E 會開啟的那個面板 (背包)

    private bool isMenuOpen = false;
    private bool hasShownTutorial = false; // 記錄是否已經教過

    void Start()
    {
        HideAllPanels();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (InventoryManager.Instance != null && !InventoryManager.Instance.isUnlocked)
            {
                Debug.Log("劇情還沒到，不能開啟背包！");
                return; // 直接擋下來，不執行後面的開關動作
            }

            // 下面是原本正常的開關邏輯
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

    public void ShowDefaultPanel()
    {
        HideAllPanels();

        if (defaultPanel != null)
        {
            defaultPanel.SetActive(true);
            isMenuOpen = true;

            // 觸發教學邏輯
            if (!hasShownTutorial)
            {
                if (TutorialManager.Instance != null)
                {
                    Debug.Log("第一次打開背包，觸發教學！");
                    hasShownTutorial = true;
                    TutorialManager.Instance.OpenTutorial();
                }
            }
        }
    }

    public void SwitchToPanel(GameObject panelToShow)
    {
        HideAllPanels();
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            isMenuOpen = true;
        }
    }

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