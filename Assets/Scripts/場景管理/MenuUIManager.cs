// MenuUIManager.cs (建議您將檔名也修正)
using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    // 將所有需要管理的 Panel 放在一個陣列中，方便管理
    public GameObject[] menuPanels;

    // 專門指定按下 E 鍵時要優先顯示的 Panel
    public GameObject defaultPanel;

    private bool isMenuOpen = false;

    void Start()
    {
        // 遊戲一開始，隱藏所有面板
        HideAllPanels();
    }

    void Update()
    {
        // 當玩家按下 E 鍵
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 直接根據選單是否開啟來決定行為
            if (isMenuOpen)
            {
                HideAllPanels(); // 如果已開啟，就關閉
            }
            else
            {
                ShowDefaultPanel(); // 如果已關閉，就顯示預設的 Panel
            }
        }
    }

    // 顯示預設的 Panel (cluePanel)
    public void ShowDefaultPanel()
    {
        // 先確保所有面板都已關閉
        HideAllPanels();

        // 如果有設定預設面板，就顯示它
        if (defaultPanel != null)
        {
            defaultPanel.SetActive(true);
            isMenuOpen = true;
        }
    }

    // 一個通用的切換方法，取代了所有 Show...()
    public void SwitchToPanel(GameObject panelToShow)
    {
        // 先確保所有面板都已關閉
        HideAllPanels();

        // 顯示指定的面板
        panelToShow.SetActive(true);
        isMenuOpen = true;
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