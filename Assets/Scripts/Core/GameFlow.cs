using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public enum GameState { Exploring, Talking, Fighting }
    public GameState CurrentState { get; private set; } = GameState.Exploring;

    private string sceneToLoadAfterDialogue = "";

    [Header("角色控制")]
    public PlayerController playerMove;
    public PlayerControllerFight playerFight;
    // 新增：分開指定兩隻 MAI
    [Header("新手區 MAI 物件")]
    public GameObject bridgeMai;      // 橋邊那隻
    public GameObject rocketMai;      // 火箭那隻

    [Header("對話系統")]
    public DialogueController dialogue;

    [Header("場景物件")]
    public GameObject maiHelpArea;
    public GameObject enemiesRoot;   // 這裡放置隱患怪物（練習用 Databug）

    [Header("任務 / 道具")]
    public ObjectiveManager objectiveManager;    // 任務指示管理器
    public GameObject cameraSceneObject;         // 場景上的相機互動物件
    public GameObject cameraCloseupUI;
    public Item cameraItemAsset;

    [Header("MAI幫助區設定")]
    public GameObject MAIHalpPanel;

    [Header("練習場流程")]
    public string trainingFinishKnot = "training_finish";  // 練習結束後要播的 Ink 節點名

    [Header("圖像廣場設定")]
    public GameObject flyerObject;
    public Item flyerItemData;
    public GameObject flyerCloseupUI;
    public Item portfolioItemData;
    public GameObject portfolioCloseupUI;

    [Header("幻影巷設定")]
    public GameObject minigamePanel_Dandadan;   // 膽大檔的面板
    public GameObject minigamePanel_GoodFortune; // 好信福的面板
    public GameObject minigamePanel_CheapBuyer;  // 購便宜的面板


    bool practiceStarted = false;       // 用於追蹤怪物是否已生成且未被清除
    string pendingActionAfterDialogue = "";  // 對話結束後要做的動作（例如 SpawnTrainingBug）

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 檢查練習怪物是否被清除
        if (CurrentState == GameState.Fighting && practiceStarted)
        {
            if (enemiesRoot != null && enemiesRoot.transform.childCount == 0)
            {
                practiceStarted = false; // 戰鬥結束
                OnTrainingFinished();
            }
        }
    }

    // ================= 遊戲狀態管理核心 =================

    public void StartDialogue(string knotName)
    {
        Debug.Log($"GameFlow.StartDialogue({knotName})");

        if (objectiveManager)
            objectiveManager.HideObjective();

        CurrentState = GameState.Talking;

        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false);

        if (dialogue)
            dialogue.StartInkDialogue(knotName);
    }

    // ================= 對話開始/結束 (由 DialogueController 呼叫) =================

    public void OnDialogueStarted()
    {
        Debug.Log("GameFlow.OnDialogueStarted()");
        CurrentState = GameState.Talking;
        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false);
    }

    public void OnDialogueFinished()
    {
        Debug.Log("🟥 對話結束");

        // 1. 優先檢查：是否有要切換場景？
        if (!string.IsNullOrEmpty(sceneToLoadAfterDialogue))
        {
            string targetScene = sceneToLoadAfterDialogue;
            sceneToLoadAfterDialogue = "";
            Debug.Log($"切換場景至: {targetScene}");
            SceneManager.LoadScene(targetScene);
            return;
        }

        // 2. 檢查是否有「待辦事項」 (例如：進入戰鬥)
        if (pendingActionAfterDialogue == "SpawnTrainingBug")
        {
            pendingActionAfterDialogue = "";
            SpawnTrainingBug();
            return; // 如果是戰鬥，就從這裡離開，不執行下面的恢復邏輯
        }

        // *** 3. (修正) 其他普通對話：回到 Exploring ***
        // 這段必須在 if 外面，這樣普通對話結束後才會執行
        CurrentState = GameState.Exploring;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;

        // 恢復顯示常駐幫助區
        if (maiHelpArea) maiHelpArea.SetActive(true);
    }
    // 新增一個公開方法供 Ink 呼叫
    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoadAfterDialogue = sceneName;
        Debug.Log($"已預約對話結束後前往: {sceneName}");
    }
    // ================= Ink 外部指令接收器 =================

    // ~ show_objective("目標", "提示")
    public void ShowObjectiveUI(string content)
    {
        Debug.Log($"Setting Objective: {content}");
        if (objectiveManager)
            objectiveManager.ShowObjective(content);
    }

    // ~ give_camera()
    public void GiveCamera()
    {
        if (cameraSceneObject != null)
        {
            cameraSceneObject.SetActive(true);
            Debug.Log("相機物件已出現在場景中，請去撿取！");
        }
        else
        {
            Debug.LogError("GameFlow: cameraSceneObject 沒有指定！");
        }
    }
    public void GetCameraItem()
    {
        if (cameraItemAsset != null)
        {
            // 1. 加入背包
            InventoryManager.Instance.Add(cameraItemAsset);

            // 2. 顯示大圖
            if (cameraCloseupUI != null)
            {
                cameraCloseupUI.SetActive(true);
            }

            // 3. 讓地上的物件消失
            if (cameraSceneObject != null)
            {
                cameraSceneObject.SetActive(false);
            }
        }
    }

    // ~ spawn_wave()
    public void SetSpawnTrainingBugAfterDialogue()
    {
        pendingActionAfterDialogue = "SpawnTrainingBug";
    }

    // ================= 練習場戰鬥 =================

    void SpawnTrainingBug()
    {
        if (enemiesRoot == null)
        {
            Debug.LogError("EnemiesRoot 根物件沒有指定！");
            return;
        }

        Debug.Log("Starting training combat...");

        enemiesRoot.SetActive(true);     // 場景裡預擺好的 Databug 出現

        CurrentState = GameState.Fighting;
        practiceStarted = true;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = true;
    }

    void OnTrainingFinished()
    {
        Debug.Log("Training Complete");

        CurrentState = GameState.Exploring;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;
        if (enemiesRoot) enemiesRoot.SetActive(false);

        if (dialogue && !string.IsNullOrEmpty(trainingFinishKnot))
            dialogue.StartInkDialogue(trainingFinishKnot);
    }

    // 之後要用可以從這裡叫 World Map
    public void ShowWorldMap()
    {
        Debug.Log("Opening World Map for selection...");
    }
    //---圖像廣場---
    // 1. 顯示傳單 (由 plaza_leah 呼叫)
    public void ShowFlyerInScene()
    {
        if (flyerObject)
        {
            flyerObject.SetActive(true);
            Debug.Log("傳單出現在地上了！");
        }
    }

    // 2. 獲得傳單 (由 plaza_flyer_pickup 呼叫)
    public void GetFlyerItem()
    {
        if (flyerItemData)
        {
            InventoryManager.Instance.Add(flyerItemData);
            if (flyerCloseupUI != null)
            {
                flyerCloseupUI.SetActive(true);
            }
        }
    }

    // 3. 銷毀傳單 (由 plaza_flyer_pickup 結束時呼叫)
    public void DestroyFlyerObject()
    {
        if (flyerObject)
        {
            // 只是隱藏它，或者 Destroy 都可以
            flyerObject.SetActive(false);
            // Destroy(flyerObject); 
        }
    }
    // 4. 獲得作品集 (由 plaza_leah_flyer 呼叫)
    public void GetPortfolioItem()
    {
        if (portfolioItemData != null)
        {
            bool success = InventoryManager.Instance.Add(portfolioItemData);
            if (portfolioCloseupUI != null)
            {
                portfolioCloseupUI.SetActive(true);
            }
            if (success) Debug.Log("獲得作品集！");
        }
        else
        {
            Debug.LogError("GameFlow: portfolioItemData 未設定！");
        }
    }
    //---幻影巷---
    public void StartMAIHelp()
    {
        Debug.Log("幫助區出現");
        if (MAIHalpPanel != null)
        {
            MAIHalpPanel.SetActive(true);

        }
        else
        {
            Debug.LogError("MAIHalpPanel 未設定！");
        }
    }
    public void StartCompareMinigame(string id)
    {
        Debug.Log($"開啟找碴小遊戲，ID: {id}");

        // 先關閉所有小遊戲面板 (防呆)
        if (minigamePanel_Dandadan) minigamePanel_Dandadan.SetActive(false);
        if (minigamePanel_GoodFortune) minigamePanel_GoodFortune.SetActive(false);
        if (minigamePanel_CheapBuyer) minigamePanel_CheapBuyer.SetActive(false);

        // 根據 ID 開啟對應的面板
        switch (id)
        {
            case "dandadan":
                if (minigamePanel_Dandadan) minigamePanel_Dandadan.SetActive(true);
                break;
            case "good_fortune":
                if (minigamePanel_GoodFortune) minigamePanel_GoodFortune.SetActive(true);
                break;
            case "cheap_buyer":
                if (minigamePanel_CheapBuyer) minigamePanel_CheapBuyer.SetActive(true);
                break;
            default:
                Debug.LogError($"GameFlow: 找不到 ID 為 '{id}' 的小遊戲面板！");
                break;
        }
    }

    public void HideMai(string id)
    {
        switch (id)
        {
            case "bridge":
                if (bridgeMai != null)
                    bridgeMai.SetActive(false);
                break;

            case "rocket":
                if (rocketMai != null)
                    rocketMai.SetActive(false);
                break;

            case "all":
                // 如果之後有需要一次全部關掉可以用
                if (bridgeMai != null) bridgeMai.SetActive(false);
                if (rocketMai != null) rocketMai.SetActive(false);
                break;

            default:
                Debug.LogWarning($"HideMai 收到未知 id: {id}");
                break;
        }
    }


}
