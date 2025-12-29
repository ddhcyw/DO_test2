using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public enum GameState { Exploring, Talking, Fighting }
    public GameState CurrentState { get; private set; } = GameState.Exploring;

    private string sceneToLoadAfterDialogue = "";

    [Header("Scene Start Dialogue")]
    public bool playStartDialogueOnSceneStart = false;
    public string startDialogueKnot = "";
    public string startDialogueOnceKey = ""; // 可留空，不留空就只播一次

    [Header("角色控制")]
    public PlayerController playerMove;
    public PlayerControllerFight playerFight;

    [Header("新手區 MAI 物件")]
    public GameObject bridgeMai;      // 橋邊那隻
    public GameObject rocketMai;      // 火箭那隻

    [Header("對話系統")]
    public DialogueController dialogue;

    [Header("UI Control")]
    public GameObject panelToHideDuringDialogue;

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

    [Header("作品集偷偷 - 辯論 Boss 戰")]
    public GameObject debatePanel;      
    public GameObject popupSuccess;    
    public GameObject popupFail;       

    // 用於記錄這一回合的正確答案 (由 Ink 指定)
    private string currentCorrectAnswer = "";

    // 用於追蹤怪物是否已生成且未被清除
    bool practiceStarted = false;
    string pendingActionAfterDialogue = "";  // 對話結束後要做的動作

    // 線索資料庫 (使用 HashSet 避免重複)
    private readonly HashSet<string> clues = new HashSet<string>();

    // ============================================================
    // Unity Lifecycle
    // ============================================================
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("GameFlow.Start() fired: " + gameObject.name);
        if (!playStartDialogueOnSceneStart) return;
        if (string.IsNullOrEmpty(startDialogueKnot)) return;

        // 如果有填 key，就做「只播一次」
        if (!string.IsNullOrEmpty(startDialogueOnceKey))
        {
            if (PlayerPrefs.GetInt(startDialogueOnceKey, 0) == 1) return;
            PlayerPrefs.SetInt(startDialogueOnceKey, 1);
            PlayerPrefs.Save();
        }

        StartDialogue(startDialogueKnot);
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

    // ============================================================
    // 遊戲狀態與對話核心
    // ============================================================

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

    // 由 DialogueController 呼叫
    public void OnDialogueStarted()
    {
        Debug.Log("GameFlow.OnDialogueStarted()");
        CurrentState = GameState.Talking;
        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false);
        if (panelToHideDuringDialogue != null)
        {
            panelToHideDuringDialogue.SetActive(false);
        }

        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.EnableMovement(false);
    }

    // 由 DialogueController 呼叫
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

        // 3. 其他普通對話：回到 Exploring
        CurrentState = GameState.Exploring;
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.EnableMovement(true);
        
        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;

        // 恢復顯示常駐幫助區
        if (maiHelpArea) maiHelpArea.SetActive(true);

        if (panelToHideDuringDialogue != null)
        {
            panelToHideDuringDialogue.SetActive(true);
        }
    }

    // 新增一個公開方法供 Ink 呼叫
    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoadAfterDialogue = sceneName;
        Debug.Log($"已預約對話結束後前往: {sceneName}");
    }

    // ============================================================
    // Ink 外部指令接收器 (External Functions)
    // ============================================================

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

    // ~ add_clue("id")
    public void AddClue(string clueID)
    {
        AddClueLocal(clueID);
    }

    public void GetCameraItem()
    {
        if (cameraItemAsset != null)
        {
            InventoryManager.Instance.Add(cameraItemAsset);
            if (cameraCloseupUI != null) cameraCloseupUI.SetActive(true);
            if (cameraSceneObject != null) cameraSceneObject.SetActive(false);
        }
    }

    // ~ spawn_wave()
    public void SetSpawnTrainingBugAfterDialogue()
    {
        pendingActionAfterDialogue = "SpawnTrainingBug";
    }

    // ============================================================
    // 練習場戰鬥邏輯
    // ============================================================

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

    // ============================================================
    // 圖像廣場 & 道具邏輯
    // ============================================================

    public void ShowFlyerInScene()
    {
        if (flyerObject)
        {
            flyerObject.SetActive(true);
            Debug.Log("傳單出現在地上了！");
        }
    }

    public void GetFlyerItem()
    {
        if (flyerItemData)
        {
            InventoryManager.Instance.Add(flyerItemData);
            if (flyerCloseupUI != null) flyerCloseupUI.SetActive(true);
        }
    }

    public void DestroyFlyerObject()
    {
        if (flyerObject) flyerObject.SetActive(false);
    }

    public void GetPortfolioItem()
    {
        if (portfolioItemData != null)
        {
            bool success = InventoryManager.Instance.Add(portfolioItemData);
            if (portfolioCloseupUI != null) portfolioCloseupUI.SetActive(true);
            if (success) Debug.Log("獲得作品集！");
        }
        else
        {
            Debug.LogError("GameFlow: portfolioItemData 未設定！");
        }
    }

    // ============================================================
    // 幻影巷邏輯
    // ============================================================

    public void StartMAIHelp()
    {
        Debug.Log("幫助區出現");
        if (MAIHalpPanel != null) MAIHalpPanel.SetActive(true);
        else Debug.LogError("MAIHalpPanel 未設定！");
    }

    public void StartCompareMinigame(string id)
    {
        Debug.Log($"開啟找碴小遊戲，ID: {id}");

        if (minigamePanel_Dandadan) minigamePanel_Dandadan.SetActive(false);
        if (minigamePanel_GoodFortune) minigamePanel_GoodFortune.SetActive(false);
        if (minigamePanel_CheapBuyer) minigamePanel_CheapBuyer.SetActive(false);

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
                if (bridgeMai != null) bridgeMai.SetActive(false);
                break;
            case "rocket":
                if (rocketMai != null) rocketMai.SetActive(false);
                break;
            case "all":
                if (bridgeMai != null) bridgeMai.SetActive(false);
                if (rocketMai != null) rocketMai.SetActive(false);
                break;
            default:
                Debug.LogWarning($"HideMai 收到未知 id: {id}");
                break;
        }
    }

    // ============================================================
    // 作品集偷偷 (辯論戰) 
    // ============================================================

    // 1. Ink 呼叫這個，設定這一回合的正確答案
    public void StartDebateRound(string answerID)
    {
        Debug.Log($"辯論回合開始，正確答案是: {answerID}");
        currentCorrectAnswer = answerID;

        // 凍結對話 (不讓玩家點擊背景觸發下一句)，並開啟面板
        if (dialogue) dialogue.enabled = false;

        if (debatePanel) debatePanel.SetActive(true);
        if (popupSuccess) popupSuccess.SetActive(false);
        if (popupFail) popupFail.SetActive(false);

        CurrentState = GameState.Talking;
    }

    // 2. UI 按鈕呼叫這個 (每個按鈕固定傳自己的 ID)
    public void OnClickDebateButton(string clickedID)
    {
        if (clickedID == currentCorrectAnswer)
        {
            Debug.Log("答對了！(駁回)");
            if (popupSuccess) popupSuccess.SetActive(true);
        }
        else
        {
            Debug.Log("答錯了... (被黑霧吞噬)");
            if (popupFail) popupFail.SetActive(true);
        }
    }

    // 3. 答對確認
    public void OnDebateSuccessConfirm()
    {
        CloseDebateUI();
        if (dialogue)
        {
            dialogue.enabled = true;

            // 判斷剛才的正確答案是什麼，決定劇情去向
            // 這樣就不怕 Ink 變數被重置了
            if (currentCorrectAnswer == "copy_machine")
            {
                // 剛才解的是影印機 -> 去第一回合成功劇情
                dialogue.StartInkDialogue("debate_success_1");
            }
            else if (currentCorrectAnswer == "canvas")
            {
                // 剛才解的是畫布 -> 去第二回合成功劇情
                dialogue.StartInkDialogue("debate_success_2");
            }
            else if (currentCorrectAnswer == "pc")
            {
                // 剛才解的是電腦 -> 去第三回合成功劇情
                dialogue.StartInkDialogue("debate_success_3");
            }
            else
            {
                Debug.LogWarning("未知的辯論進度，預設跳轉");
                dialogue.StartInkDialogue("debate_success_1");
            }
        }
    }

    // 4. 答錯確認
    public void OnDebateFailConfirm()
    {
        CloseDebateUI();
        ClearAllClues();

        if (dialogue)
        {
            dialogue.enabled = true;
            SetSceneToLoad("幻影巷Scene");
            Debug.Log("播放失敗劇情...");
            dialogue.StartInkDialogue("debate_failed");
        }
        else
        {
            SceneManager.LoadScene("幻影巷Scene");
        }
    }

    void CloseDebateUI()
    {
        if (debatePanel) debatePanel.SetActive(false);
        if (popupSuccess) popupSuccess.SetActive(false);
        if (popupFail) popupFail.SetActive(false);
    }

    // ============================================================
    // 線索管理系統 (Clue System)
    // ============================================================

    public void AddClueLocal(string clueID)
    {
        if (string.IsNullOrEmpty(clueID)) return;
        clues.Add(clueID);
        Debug.Log($"Clue unlocked: {clueID}");
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.UnlockClue(clueID);
        }
    }

    public bool HasClue(string clueID)
    {
        return !string.IsNullOrEmpty(clueID) && clues.Contains(clueID);
    }

    // 檢查是否收集齊全基地要的三個線索
    public bool HasAllBaseClues()
    {
        return HasClue("clue_pc") && HasClue("clue_copy_machine") && HasClue("clue_canvas");
    }

    // 如果你要測試用，一鍵清掉
    public void ClearAllClues()
    {
        clues.Clear();
        Debug.Log("All clues cleared");
    }
}