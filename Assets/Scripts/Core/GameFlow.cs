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
    public GameObject enemiesRoot;   // 之後練習場多隻可以用這個

    // 🔹 新增：練習用 Databug
    [Header("Training Bug")]
    public GameObject trainingBugPrefab;     // 指到 Databug prefab
    public Transform trainingBugSpawnPoint;  // 指到 TrainingBugSpawnPoint
    GameObject currentTrainingBug;

    // 對話結束後要做的動作（簡單版）
    string pendingActionAfterDialogue = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ================= 對話開始 =================
    public void OnDialogueStarted()
    {
        Debug.Log("🟦 GameFlow.OnDialogueStarted()");

        CurrentState = GameState.Talking;

        if (playerMove)  playerMove.enabled  = false;
        if (playerFight) playerFight.enabled = false;

        if (maiHelpArea) maiHelpArea.SetActive(false);
    }

    // 提供給別人（例如 MAI2 的觸發區）設定：
    // 「這段對話結束後要生出練習 Databug」
    public void SetSpawnTrainingBugAfterDialogue()
    {
        pendingActionAfterDialogue = "SpawnTrainingBug";
    }

    // ================= 對話結束 =================
    public void OnDialogueFinished()
    {
        Debug.Log("🟥 GameFlow.OnDialogueFinished()");

        // 如果這次對話結束後要生出練習 Databug
        if (pendingActionAfterDialogue == "SpawnTrainingBug")
        {
            pendingActionAfterDialogue = "";
            SpawnTrainingBug();
            return;
        }

        // 其他對話：回到 Exploring
        CurrentState = GameState.Exploring;

        if (playerMove)  playerMove.enabled  = true;
        if (playerFight) playerFight.enabled = false;
    }

    // ================= 生出 Databug，進入練習戰鬥 =================
    void SpawnTrainingBug()
    {
        if (currentTrainingBug != null)
        {
            Debug.Log("Training bug already exists.");
            return;
        }

        if (!trainingBugPrefab || !trainingBugSpawnPoint)
        {
            Debug.LogError("TrainingBug prefab 或 spawnPoint 沒有指定！");
            return;
        }

        Debug.Log("🐛 Spawn training Databug");

        currentTrainingBug = Instantiate(
            trainingBugPrefab,
            trainingBugSpawnPoint.position,
            Quaternion.identity
        );

        CurrentState = GameState.Fighting;

        // 看你要不要讓玩家可走路，如果要可以保留 playerMove.enabled = true
        if (playerMove)  playerMove.enabled  = true;
        if (playerFight) playerFight.enabled = true;
    }

    // ================= 被玩家打死時呼叫 =================
    public void OnTrainingBugKilled()
    {
        Debug.Log("✅ Training Databug killed");

        CurrentState = GameState.Exploring;

        if (playerMove)  playerMove.enabled  = true;
        if (playerFight) playerFight.enabled = false;

        currentTrainingBug = null;

        // 播放對話三（Ink 裡的 training_finish knot）
        if (dialogue)
            dialogue.StartInkDialogue("training_finish");
    }
}
