using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI 設定")]
    public GameObject tutorialRootPanel; // 整個教學的最上層父物件
    public List<GameObject> tutorialSteps; // Step1, Step2, Step3...
    [Tooltip("點擊相機任務第幾步(要-1)")]
    public int clickStepIndex = 5;
    [Header("拖曳任務第幾步(要-1)")]
    public int dragStepIndex = 7;
    [Header("拍照任務第幾步(要-1)")]
    public int takePhotoStepIndex = 13;

    [Header("狀態")]
    // 公開這個變數，讓其他腳本可以讀取
    public int CurrentStepIndex = 0;

    // 判斷教學是否正在進行中
    public bool IsTutorialActive => tutorialRootPanel != null && tutorialRootPanel.activeSelf;

    void Awake()
    {
        Instance = this;
        if (tutorialRootPanel) tutorialRootPanel.SetActive(false);
    }

    public void OpenTutorial()
    {
        if (tutorialRootPanel != null) tutorialRootPanel.SetActive(true);
        CurrentStepIndex = 0;
        UpdateStepVisuals();
    }

    public void NextStep()
    {
        if (CurrentStepIndex < tutorialSteps.Count - 1)
        {
            CurrentStepIndex++;
            Debug.Log($"教學進入第 {CurrentStepIndex + 1} 步");
            UpdateStepVisuals();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void UpdateStepVisuals()
    {
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i] != null)
                tutorialSteps[i].SetActive(i == CurrentStepIndex);
        }
    }

    private void CompleteTutorial()
    {
        Debug.Log("教學完成！");
        if (tutorialRootPanel != null) tutorialRootPanel.SetActive(false);
    }
}