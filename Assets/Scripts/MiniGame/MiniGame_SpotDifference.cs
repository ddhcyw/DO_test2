using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 必須引用這個才能用 Coroutine

public class MiniGame_SpotDifference : MonoBehaviour
{
    [Header("遊戲設定")]
    public Button[] targetButtons;
    public GameObject[] scoreIcons;

    [Header("獲勝後設定")]
    public bool closeOnWin = true;
    public float winDelay = 1.5f; // 新增：獲勝後延遲幾秒才關閉
    public string winInkKnot = "minigame_success";

    private int currentScore = 0;
    private bool isGameFinished = false; // 防止在延遲期間重複觸發

    void OnEnable()
    {
        ResetGame();
    }

    void ResetGame()
    {
        currentScore = 0;
        isGameFinished = false;

        foreach (var icon in scoreIcons)
        {
            if (icon != null) icon.SetActive(false);
        }

        foreach (var btn in targetButtons)
        {
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                Button tempBtn = btn;
                btn.onClick.AddListener(() => OnCorrectButtonClicked(tempBtn));
            }
        }
    }

    void OnCorrectButtonClicked(Button clickedBtn)
    {
        // 如果遊戲已經結束(正在跑延遲)，就不處理點擊
        if (isGameFinished) return;

        clickedBtn.interactable = false;

        // 顯示積分圖示
        if (currentScore < scoreIcons.Length)
        {
            if (scoreIcons[currentScore] != null)
            {
                scoreIcons[currentScore].SetActive(true);
            }
        }

        currentScore++;

        // 檢查是否過關
        if (currentScore >= targetButtons.Length)
        {
            // 啟動獲勝流程
            StartCoroutine(WinSequence());
        }
    }

    // 新增：處理延遲的協程
    IEnumerator WinSequence()
    {
        isGameFinished = true; // 鎖定狀態
        Debug.Log("過關！等待延遲...");

        // 這裡就是延遲的地方 (等待 winDelay 秒)
        yield return new WaitForSeconds(winDelay);

        // 延遲結束，執行原本的獲勝邏輯

       
        // 關閉面板
        if (closeOnWin)
        {
            gameObject.SetActive(false);
        }
    }
}