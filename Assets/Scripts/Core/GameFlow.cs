using UnityEngine;
using System.Collections; // Required for StartCoroutine, though not strictly used in this final version

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public enum GameState { Exploring, Talking, Fighting }
    public GameState CurrentState { get; private set; } = GameState.Exploring;

    [Header("角色控制")]
    public PlayerController playerMove;
    public PlayerControllerFight playerFight;

    [Header("對話系統")]
    public DialogueController dialogue;

    [Header("場景物件")]
    public GameObject maiHelpArea;
    public GameObject enemiesRoot;   // 這裡放置隱患怪物，它們需要 Health.cs 和 Photographable.cs

    // 🔹 新增：UI/Item & 狀態追蹤
    [Header("Game State Tracking")]
    public ObjectiveManager objectiveManager;    // 任務指示管理器
    public GameObject cameraSceneObject;
    private bool practiceStarted = false;       // 用於追蹤怪物是否已生成且未被清除

    [Header("Training Bug")]
    public GameObject trainingBugPrefab;     // 指到 Databug prefab
    public Transform trainingBugSpawnPoint;  // 指到 TrainingBugSpawnPoint
    GameObject currentTrainingBug; // 單一訓練怪物的實例

    // 對話結束後要做的動作（簡單版）
    string pendingActionAfterDialogue = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 檢查訓練怪物是否被清除 (GDD 步驟 4/5)
        if (CurrentState == GameState.Fighting && practiceStarted)
        {
            // 檢查 EnemiesRoot 底下是否還有怪物 (如果淨化相機會 Destroy 怪物)
            if (enemiesRoot != null && enemiesRoot.transform.childCount == 0)
            {
                practiceStarted = false; // 戰鬥結束
                OnTrainingFinished(); // 觸發練習完成的流程
            }
        }
    }


    // ================= 遊戲狀態管理核心 =================

    // Modified: 統一啟動點
    public void StartDialogue(string knotName)
    {
        Debug.Log($"GameFlow.StartDialogue({knotName})");

        // 隱藏舊任務 UI (在開始新任務時清除舊的目標)
        if (objectiveManager)
            objectiveManager.HideObjective();

        // 設置狀態
        CurrentState = GameState.Talking;

        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false);

        // 啟動對話 (使用 Ink knot name)
        if (dialogue)
            dialogue.StartInkDialogue(knotName);
    }

    // ================= 對話開始/結束 (由 DialogueController 呼叫) =================

    public void OnDialogueStarted()
    {
        // Redundant method, but preserved for compatibility
        Debug.Log("GameFlow.OnDialogueStarted() - Deprecated");
        CurrentState = GameState.Talking;
        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false);
    }

    public void OnDialogueFinished()
    {
        Debug.Log("GameFlow.OnDialogueFinished()");

        // 檢查是否有待執行的動作 (例如：Ink script 呼叫了 ~ spawn_wave())
        if (pendingActionAfterDialogue == "SpawnTrainingBug")
        {
            pendingActionAfterDialogue = "";
            SpawnTrainingBug(); // GDD 步驟 4: 進入戰鬥狀態
            return;
        }

        // 其他對話：回到 Exploring
        CurrentState = GameState.Exploring;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;
    }


    // ================= Ink 外部指令接收器 =================

    // GDD 步驟 2/5 - 顯示任務目標 (Ink: ~ show_objective)
    public void ShowObjectiveUI(string target, string hint)
    {
        Debug.Log($"Setting Objective: {target}");
        if (objectiveManager)
            objectiveManager.ShowObjective(target, hint);
    }

    // GDD 步驟 3 - 獲得相機 (Ink: ~ give_camera())
    public void GiveCamera()
    {
        // 2. (修改) 邏輯改為：顯示場景上的物件
        if (cameraSceneObject != null)
        {
            cameraSceneObject.SetActive(true); // 讓它出現！
            Debug.Log("相機物件已出現在場景中，請去撿取！");

        }
        else
        {
            Debug.LogError("GameFlow: cameraSceneObject 沒有指定！");
        }

    }

    // GDD 步驟 4 - 設置戰鬥旗標 (Ink: ~ spawn_wave())
    public void SetSpawnTrainingBugAfterDialogue()
    {
        pendingActionAfterDialogue = "SpawnTrainingBug";
    }

    // ================= 練習場戰鬥追蹤 (內部/被動呼叫) =================

    void SpawnTrainingBug()
    {
        // GDD 步驟 4: 練習場跳出三隻「隱患怪物」 (我們使用 EnemiesRoot 來管理)

        if (enemiesRoot == null)
        {
            Debug.LogError("EnemiesRoot 根物件沒有指定！");
            return;
        }

        Debug.Log("Starting training combat...");

        // 啟用敵人根物件 (假設怪物都在裡面，且 Health.cs 和 Photographable.cs 都已設定)
        enemiesRoot.SetActive(true);

        CurrentState = GameState.Fighting;
        practiceStarted = true; // 開始追蹤戰鬥狀態

        if (playerMove) playerMove.enabled = true; // 允許玩家移動
        if (playerFight) playerFight.enabled = true; // 允許玩家戰鬥
    }

    // GDD 步驟 5 - 怪物被清除後呼叫 (由 Update() 偵測到)
    void OnTrainingFinished()
    {
        Debug.Log("Training Complete");

        CurrentState = GameState.Exploring;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;
        if (enemiesRoot) enemiesRoot.SetActive(false); // 隱藏敵人根物件

        // GDD 步驟 5: 播放練習完成對話
        if (dialogue)
            dialogue.StartInkDialogue("practice_complete");
    }

    // 任務完成後，開啟地圖 (GDD 步驟 5)
    public void ShowWorldMap()
    {
        // GDD: 開啟地圖 (這需要您的 MenuUIManager 來處理)
        // 例如: MenuUIManager.Instance.SwitchToPanel(mapPanel); 
        Debug.Log("Opening World Map for selection...");
    }
}