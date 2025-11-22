using UnityEngine;
using UnityEngine.UI;

public class OpeningSequenceManager : MonoBehaviour
{
    [Header("設定")]
    public GameFlow gameFlow; // 引用 GameFlow 來控制玩家能否移動
    public GameObject uiRoot; // 整個開場 UI 的父物件 (用來最後關閉自己)

    [Header("第一階段：開場動畫 (3張圖)")]
    public Image storyImageDisplay; // 用來顯示圖片的 Image 元件
    public Sprite[] storySprites;   // 放那 3 張開場圖
    public GameObject storyPanel;   // 放圖片的 Panel

    [Header("第二階段：教學面板 (3個Panel)")]
    public GameObject[] tutorialPanels; // 放那 3 個教學 Panel

    [Header("MAI 幫助區 (要確保隱藏)")]
    public GameObject maiHelpArea;

    [Header("場景效果")]
    public GameObject dimmerObject;

    // 內部計數器
    private int currentStep = 0;
    private int totalSteps = 0;

    void Start()
    {
        // 1. 計算總步數 (圖片數 + 面板數)
        totalSteps = storySprites.Length + tutorialPanels.Length;

        // 2. 鎖定玩家 (不讓 GameFlow 運作)
        if (gameFlow)
        {
            gameFlow.playerMove.enabled = false;
            // 如果有其他的控制也可以在這裡關閉
        }

        // 3. 確保 MAI 幫助區是關閉的
        if (maiHelpArea) maiHelpArea.SetActive(false);

        // 4. 初始化顯示
        UpdateDisplay();
    }

    void Update()
    {
        // 偵測滑鼠左鍵點擊
        if (Input.GetMouseButtonDown(0))
        {
            currentStep++; // 下一步

            if (currentStep >= totalSteps)
            {
                EndSequence(); // 結束
            }
            else
            {
                UpdateDisplay(); // 更新畫面
            }
        }
    }

    void UpdateDisplay()
    {
        // --- 第一階段：顯示圖片 ---
        if (currentStep < storySprites.Length)
        {
            storyPanel.SetActive(true);

            // 隱藏所有教學面板
            foreach (var p in tutorialPanels) if (p) p.SetActive(false);

            // *** 這裡就是消失的那行！把它加回來！ ***
            if (storyImageDisplay && storySprites[currentStep])
            {
                storyImageDisplay.sprite = storySprites[currentStep];
            }

            // 開啟變暗遮罩
            if (dimmerObject) dimmerObject.SetActive(true);
        }
        // --- 第二階段：顯示教學 Panel ---
        else
        {
            storyPanel.SetActive(false); // 關閉圖片區

            // 確保遮罩開啟
            if (dimmerObject) dimmerObject.SetActive(true);

            // 計算現在是第幾個教學面板
            int tutorialIndex = currentStep - storySprites.Length;

            // 顯示對應的面板，隱藏其他的
            for (int i = 0; i < tutorialPanels.Length; i++)
            {
                if (tutorialPanels[i])
                    tutorialPanels[i].SetActive(i == tutorialIndex);
            }
        }
    }

    void EndSequence()
    {
        Debug.Log("開場結束，進入遊戲！");

        // 1. 解鎖玩家控制
        if (gameFlow) gameFlow.playerMove.enabled = true;

        // *** 修正：開場結束後，要「顯示」常駐的幫助區 ***
        if (maiHelpArea) maiHelpArea.SetActive(true);

        // 3. 關閉開場 UI
        if (uiRoot) uiRoot.SetActive(false);

        Destroy(gameObject);
    }
}