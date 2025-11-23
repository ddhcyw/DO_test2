using UnityEngine;

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
    public GameObject enemiesRoot;   // 這裡放置隱患怪物（練習用 Databug）

    [Header("任務 / 道具")]
    public ObjectiveManager objectiveManager;    // 任務指示管理器
    public GameObject cameraSceneObject;         // 場景上的相機互動物件

    [Header("練習場流程")]
    public string trainingFinishKnot = "training_finish";  // 練習結束後要播的 Ink 節點名

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
        Debug.Log("GameFlow.OnDialogueFinished()");

        // 如果 Ink 剛剛叫了 ~ spawn_wave()
        if (pendingActionAfterDialogue == "SpawnTrainingBug")
        {
            pendingActionAfterDialogue = "";
            SpawnTrainingBug();
            return;
        }

        // 其他：回到 Exploring
        CurrentState = GameState.Exploring;

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;
    }

    // ================= Ink 外部指令接收器 =================

    // ~ show_objective("目標", "提示")
    public void ShowObjectiveUI(string target, string hint)
    {
        Debug.Log($"Setting Objective: {target}");
        if (objectiveManager)
            objectiveManager.ShowObjective(target, hint);
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
}
