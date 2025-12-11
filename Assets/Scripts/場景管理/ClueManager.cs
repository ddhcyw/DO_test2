using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 必須引用 UI 命名空間

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }
    // 定義一個類別來儲存單個線索的資料
    [System.Serializable]
    public class ClueData
    {
        public string clueID;          // 線索的唯一 ID (例如 "Key", "Letter")
        public Button uiButton;        // 對應的 UI 按鈕
        public Sprite unlockedIcon;    // 解鎖後顯示的圖示 (彩色/可點擊狀態)
        public Sprite detailImage;     // 點擊後顯示的詳細提示大圖
        [HideInInspector] public bool isUnlocked = false; // 是否已獲得
    }

    [Header("UI 設定")]
    public List<ClueData> clueList;    // 在 Inspector 中設定所有的線索
    public GameObject detailPanel;     // 顯示大圖的面板 (初始應設為隱藏)
    public Image detailImageDisplay;   // 面板中用來顯示大圖的 Image 元件
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
            DontDestroyOnLoad(gameObject); // 切換場景時保留線索狀態
        }
    }
    void Start()
    {
        // 初始化：關閉詳細面板，並將所有按鈕設為不可互動（或鎖定圖示）
        detailPanel.SetActive(false);

        foreach (var clue in clueList)
        {
            // 預設按鈕不能按
            clue.uiButton.interactable = false;

            // 這裡可以選擇是否要在開始時將圖示設為黑色或問號
            // clue.uiButton.image.sprite = lockedSprite; 
        }
    }

    // --- 核心功能 1: 獲得線索 ---
    // 當玩家撿到道具時，呼叫此方法，傳入線索 ID
    public void UnlockClue(string id)
    {
        // 尋找符合 ID 的線索
        ClueData foundClue = clueList.Find(x => x.clueID == id);

        if (foundClue != null && !foundClue.isUnlocked)
        {
            foundClue.isUnlocked = true;

            // 1. 更改按鈕圖片為「已獲得」的樣子
            foundClue.uiButton.image.sprite = foundClue.unlockedIcon;

            // 2. 讓按鈕可以被點擊
            foundClue.uiButton.interactable = true;

            // 3. 綁定點擊事件：點下去後顯示這張線索的大圖
            // 先移除舊的監聽器避免重複，再加入新的
            foundClue.uiButton.onClick.RemoveAllListeners();
            foundClue.uiButton.onClick.AddListener(() => ShowDetail(foundClue));

            Debug.Log($"線索 {id} 已解鎖！");
        }
        else
        {
            Debug.LogWarning($"找不到線索 ID: {id}");
        }
    }

    // --- 核心功能 2: 顯示詳細大圖 ---
    void ShowDetail(ClueData clue)
    {
        // 設定大圖內容
        detailImageDisplay.sprite = clue.detailImage;
        // 開啟面板
        detailPanel.SetActive(true);
    }

    

    // --- 測試用 (開發時使用) ---
    void Update()
    {
        // 按下 A 鍵模擬獲得線索 "Key"
        if (Input.GetKeyDown(KeyCode.A))
        {
            UnlockClue("Key");
        }
    }
}