using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 將你所有的 UI Panel 拖曳到這裡
    public GameObject mainMenuPanel;
    public GameObject albumPanel;
    public GameObject taskPanel;
    public GameObject talkPanel;

    void Start()
    {
        // 遊戲一開始只顯示主選單面板
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        albumPanel.SetActive(false);
        taskPanel.SetActive(false);
        talkPanel.SetActive(false);
    }

    public void ShowAlbum()
    {
        mainMenuPanel.SetActive(false);
        albumPanel.SetActive(true);
        taskPanel.SetActive(false);
        talkPanel.SetActive(false);
    }

    public void ShowTask()
    {
        mainMenuPanel.SetActive(false);
        albumPanel.SetActive(false);
        taskPanel.SetActive(true);
        talkPanel.SetActive(false);
    }

    // 你可以為其他需要隱藏所有面板的事件建立一個方法
    public void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        albumPanel.SetActive(false);
        taskPanel.SetActive(false);
        talkPanel.SetActive(false);
    }
    public void ShowTalk()
    {
        mainMenuPanel.SetActive(false);
        albumPanel.SetActive(false);
        taskPanel.SetActive(false);
        talkPanel.SetActive(true);
    }
}