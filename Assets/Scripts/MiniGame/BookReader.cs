using UnityEngine;
using UnityEngine.UI;

public class BookReader : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject bookPanel;       // 整個書本介面
    public GameObject[] pages;         // 拖曳那 9 個頁面進來
    public Button nextButton;          // 下一頁按鈕
    public Button prevButton;          // 上一頁按鈕
    public Button closeButton;         // 關閉按鈕

    [Header("設定")]
    public bool pauseGameTime = true;  // 開書時是否要暫停時間

    private int currentIndex = 0;      // 目前看到第幾頁
    private string nextDialogueKnot = ""; // 關閉後要接的劇情節點名

    void Start()
    {
        // 預設關閉
        if (bookPanel) bookPanel.SetActive(false);

        // 綁定按鈕事件 (也可以在 Inspector 手動拉)
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
        if (closeButton) closeButton.onClick.AddListener(CloseBook);
    }

    // ==================================================
    // 公開功能：給 GameFlow / Ink 呼叫
    // ==================================================

    /// <param name="nextKnot">看完書後要播放的 Ink 節點名稱</param>
    public void OpenBook(string nextKnot)
    {
        nextDialogueKnot = nextKnot;
        currentIndex = 0; // 重置回第一頁

        if (bookPanel) bookPanel.SetActive(true);
        if (pauseGameTime) Time.timeScale = 0f; // 暫停時間

        UpdatePageDisplay();
    }

    // ==================================================
    // 內部邏輯：翻頁
    // ==================================================

    public void NextPage()
    {
        if (currentIndex < pages.Length - 1)
        {
            currentIndex++;
            UpdatePageDisplay();
        }
    }

    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePageDisplay();
        }
    }

    void UpdatePageDisplay()
    {
        // 1. 顯示/隱藏每一頁
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex);
        }

        // 2. 控制按鈕是否出現 (第一頁沒上一頁，最後一頁沒下一頁)
        if (prevButton) prevButton.gameObject.SetActive(currentIndex > 0);
        if (nextButton) nextButton.gameObject.SetActive(currentIndex < pages.Length - 1);
    }

    // ==================================================
    // 關閉邏輯
    // ==================================================

    public void CloseBook()
    {
        // 1. 關閉介面
        if (bookPanel) bookPanel.SetActive(false);
        if (pauseGameTime) Time.timeScale = 1f; // 恢復時間

        // 2. 觸發下一段對話
        if (!string.IsNullOrEmpty(nextDialogueKnot) && GameFlow.Instance != null)
        {
            Debug.Log($"書本關閉，接續劇情: {nextDialogueKnot}");
            GameFlow.Instance.StartDialogue(nextDialogueKnot);
        }
    }
}