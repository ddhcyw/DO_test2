using UnityEngine;

public class UIManager : MonoBehaviour
{
    // �N�A�Ҧ��� UI Panel �즲��o��
    public GameObject mainMenuPanel;
    public GameObject albumPanel;
    public GameObject taskPanel;
    public GameObject talkPanel;

    void Start()
    {
        // �C���@�}�l�u��ܥD��歱�O
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

    // �A�i�H����L�ݭn���éҦ����O���ƥ�إߤ@�Ӥ�k
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