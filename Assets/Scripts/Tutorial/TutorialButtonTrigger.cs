using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialButtonTrigger : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("這顆按鈕對應教學的第幾步？(從 0 開始)")]
    public int targetStepIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 先確認點擊有沒有發生
        Debug.Log($"【測試】點擊到了按鈕：{gameObject.name}");

        // 2. 檢查 TutorialManager 是否存在
        if (TutorialManager.Instance == null)
        {
            Debug.LogError("【失敗】找不到 TutorialManager！");
            return;
        }

        // 3. 檢查教學是否開啟
        if (!TutorialManager.Instance.IsTutorialActive)
        {
            Debug.LogWarning("【無效】教學目前是關閉狀態 (IsTutorialActive = false)");
            return;
        }

        // 4. 檢查步驟是否正確
        int currentStep = TutorialManager.Instance.CurrentStepIndex;
        Debug.Log($"【檢查狀態】目前步驟: {currentStep} / 目標步驟: {targetStepIndex}");

        if (currentStep == targetStepIndex)
        {
            Debug.Log("【成功】條件符合，進入下一步！");
            TutorialManager.Instance.NextStep();
        }
        else
        {
            Debug.LogWarning("【無效】步驟不對，現在不是按這顆按鈕的時候。");
        }
    }
}