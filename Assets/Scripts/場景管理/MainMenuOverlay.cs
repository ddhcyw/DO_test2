using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuOverlay : MonoBehaviour
{
    [Header("UI設定")]
    public Button continueButton; // 繼續遊戲按鈕
    public string firstLevelName = "(2D)新手區 1"; // 預設第一關名稱

    void Start()
    {
        // 新遊戲重載場景後直接進遊戲，不顯示選單
        if (PlayerPrefs.GetInt("_StartNewGame", 0) == 1)
        {
            PlayerPrefs.DeleteKey("_StartNewGame");
            PlayerPrefs.Save();
            StartGame();
            return;
        }

        // 1. 遊戲開始時，先暫停時間，以免怪跑來跑去
        Time.timeScale = 0f;

        // 2. 檢查存檔，決定「繼續遊戲」能不能按
        if (continueButton != null)
        {
            if (!PlayerPrefs.HasKey("SavedScene"))
                continueButton.interactable = false;
            else
                continueButton.interactable = true;
        }
    }

    // --- 按鈕：新遊戲 ---
    public void OnClickNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("_StartNewGame", 1); // 場景重載後直接進遊戲
        PlayerPrefs.Save();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearAll();

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelName);
    }

    // --- 任何場景皆可呼叫：返回主選單 ---
    public static void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("(2D)新手區 1");
    }

    // --- 按鈕：繼續遊戲 ---
    public void OnClickContinue()
    {
        // 讀取存檔的場景
        string savedScene = PlayerPrefs.GetString("SavedScene", firstLevelName);
        string currentScene = SceneManager.GetActiveScene().name;

        // 如果存檔的場景 就是 現在這個場景
        if (savedScene == currentScene)
        {
            // 直接開始
            StartGame();
        }
        else
        {
            // 如果存檔在別關（例如基地），就跳轉過去
            Time.timeScale = 1f; // 切場景前記得恢復時間
            SceneManager.LoadScene(savedScene);
        }
    }

    // --- 共用邏輯：開始遊戲 ---
    void StartGame()
    {
        // 1. 恢復時間流動
        Time.timeScale = 1f;

        // 2. 關閉自己 (隱藏主選單)
        gameObject.SetActive(false);
    }
}
