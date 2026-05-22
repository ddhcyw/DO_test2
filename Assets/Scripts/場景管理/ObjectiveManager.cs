using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI 面板切換")]
    [Tooltip("平常待機時的面板 (沒有任務時顯示這個)")]
    public GameObject idlePanel;

    [Tooltip("有新任務/提示時，彈出的面板 (Objective Panel Root)")]
    public GameObject objectivePanel;

    [Header("任務文字")]
    [Tooltip("任務面板上的文字組件")]
    public TMP_Text objectiveText;

    public static ObjectiveManager Instance { get; private set; }

    // 排隊系統
    private Queue<string> objectiveQueue = new Queue<string>();
    private bool isDisplaying = false;
    void Start()
    {
        // 換場景時，自動把這個任務面板重新綁定給當前新場景的 GameFlow
        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.objectiveManager = this;
        }
    }
    void Awake()
    {
        if (Instance == null) Instance = this;

        // 遊戲一開始，強制顯示待機面板，隱藏任務面板
        ClearObjective();
    }

    // 新增任務到排隊隊列
    public void ShowObjective(string content)
    {
        objectiveQueue.Enqueue(content);

        if (!isDisplaying)
        {
            StartCoroutine(DisplayObjectivesSequence());
        }
    }

    // 依序顯示任務的排隊處理器
    private IEnumerator DisplayObjectivesSequence()
    {
        isDisplaying = true;

        //任務來了：隱藏待機，顯示任務面板
        if (idlePanel) idlePanel.SetActive(false);
        if (objectivePanel) objectivePanel.SetActive(true);

        while (objectiveQueue.Count > 0)
        {
            string nextObj = objectiveQueue.Dequeue();

            // 更新任務面板上的文字
            if (objectiveText) objectiveText.text = nextObj;

            // 停留 2 秒讓玩家看清楚
            yield return new WaitForSeconds(5f);
        }

        // 任務全部播完
        isDisplaying = false;

        // 播完後自動切回待機狀態
        ClearObjective();
    }

    // 恢復為預設待機狀態
    public void ClearObjective()
    {
        objectiveQueue.Clear();

        //任務結束：隱藏任務，顯示待機面板
        if (objectivePanel) objectivePanel.SetActive(false);
        if (idlePanel) idlePanel.SetActive(true);
    }

    // 完全隱藏所有面板 (例如看動畫或 Boss 戰時使用)
    public void HideObjective()
    {
        if (idlePanel) idlePanel.SetActive(false);
        if (objectivePanel) objectivePanel.SetActive(false);
    }
}