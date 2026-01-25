using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    // 單例模式，讓其他腳本 (TutorialDropSlot / GameFlow) 可以方便呼叫
    public static TutorialManager Instance { get; private set; }

    [Header("UI 設定")]
    [Tooltip("教學介面的最上層父物件 (包含黑色遮罩背景)，預設會被隱藏")]
    public GameObject tutorialRootPanel;

    [Tooltip("依順序拖入教學步驟的 Panel (Step1, Step2, Step3...)")]
    public List<GameObject> tutorialSteps;

    // 內部變數：紀錄目前走到第幾步
    private int currentStepIndex = 0;

    // 狀態檢查：是否正在教學中
    public bool IsTutorialActive => tutorialRootPanel != null && tutorialRootPanel.activeSelf;

    void Awake()
    {
        // Singleton 設置
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // 遊戲一開始先關閉教學介面
        CloseTutorialUI();
    }

    // =========================================================
    // 供外部 (GameFlow / Ink) 呼叫的入口
    // =========================================================
    public void OpenTutorial()
    {
        Debug.Log("TutorialManager: 開始教學流程");

        // 1. 開啟總面板
        if (tutorialRootPanel != null) tutorialRootPanel.SetActive(true);

        // 2. 重置步驟到 0
        currentStepIndex = 0;

        // 3. 更新畫面顯示
        UpdateStepVisuals();
    }

    // =========================================================
    // 供按鈕或拖放事件呼叫
    // =========================================================
    public void NextStep()
    {
        // 如果已經結束了就不做動作
        if (!IsTutorialActive) return;

        // 檢查是否還有下一步
        if (currentStepIndex < tutorialSteps.Count - 1)
        {
            currentStepIndex++;
            Debug.Log($"教學下一步: {currentStepIndex}");
            UpdateStepVisuals();
        }
        else
        {
            // 如果已經是最後一步，再按就是結束教學
            CompleteTutorial();
        }
    }

    // =========================================================
    // 內部邏輯
    // =========================================================

    // 根據 currentStepIndex決定哪個 Panel 要開，哪個要關
    private void UpdateStepVisuals()
    {
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i] != null)
            {
                // 如果是當前步驟 -> 開啟 (true)
                // 如果不是當前步驟 -> 關閉 (false)
                tutorialSteps[i].SetActive(i == currentStepIndex);
            }
        }
    }

    private void CompleteTutorial()
    {
        Debug.Log("教學完成！");
        CloseTutorialUI();

        // 如果需要在教學結束後通知 GameFlow 或 Ink 繼續劇情，
        // 可以在這裡呼叫，例如: GameFlow.Instance.OnTutorialFinished();
    }

    private void CloseTutorialUI()
    {
        if (tutorialRootPanel != null)
        {
            tutorialRootPanel.SetActive(false);
        }
    }
}