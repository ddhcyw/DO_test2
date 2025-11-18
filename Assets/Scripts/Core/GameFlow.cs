using UnityEngine;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public enum GameState
    {
        Exploring, // 自由走路
        Talking,   // 正在對話
        Fighting   // 練習場戰鬥
    }

    // 劇情目前走到哪裡
    public enum StoryStage
    {
        None,               // 尚未開始
        MetMai1,            // 對話一結束
        MetMai2,            // 對話二結束，要進入練習場
        TrainingInProgress, // 練習場進行中
        TrainingDone        // 對話三結束，之後可以接開地圖
    }

    public GameState CurrentState { get; private set; } = GameState.Exploring;
    public StoryStage CurrentStage { get; private set; } = StoryStage.None;

    [Header("角色控制")]
    public PlayerController playerMove;
    public PlayerControllerFight playerFight;

    [Header("對話系統")]
    public DialogueController dialogue;

    [Header("場景物件")]
    public GameObject enemiesRoot;   // 數據蟲父物件（預設關掉）

    // 目前播放的是哪一段對話（用來在 OnDialogueFinished 做分支）
    string currentDialogueId = "";

    // 練習場擊殺數
    int killedEnemies = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // -------------------------------------------------
    // 讓別的腳本呼叫：開始一段故事對話
    // dialogueId：你自己命名用來識別，第1/2/3段
    // inkKnot  ：Ink 裡面對應的 knot 名稱
    // -------------------------------------------------
    public void StartStoryDialogue(string dialogueId, string inkKnot)
    {
        if (!dialogue) return;
        if (CurrentState == GameState.Talking) return; // 正在講話就先不要重複開

        currentDialogueId = dialogueId;

        // 讓 DialogueController 從指定 knot 開始
        dialogue.StartInkDialogue(inkKnot);
    }

    // ================= 對話開始（給 DialogueController 回呼） =================
    public void OnDialogueStarted()
    {
        Debug.Log("GameFlow.OnDialogueStarted");

        CurrentState = GameState.Talking;

        if (playerMove)  playerMove.enabled  = false;
        if (playerFight) playerFight.enabled = false;
    }

    // ================= 對話結束（給 DialogueController 回呼） =================
    public void OnDialogueFinished()
    {
        Debug.Log($"GameFlow.OnDialogueFinished, dialogueId = {currentDialogueId}");

        switch (currentDialogueId)
        {
            case "MAI1":
                // 對話一結束 → 回到探索，去找 MAI2
                CurrentStage = StoryStage.MetMai1;
                GoExploring();
                break;

            case "MAI2":
                // 對話二結束 → 進入練習場戰鬥
                CurrentStage = StoryStage.TrainingInProgress;
                StartTrainingFight();
                break;

            case "TRAINING_END":
                // 對話三結束 → 練習完成，回到探索（之後可接開地圖）
                CurrentStage = StoryStage.TrainingDone;
                GoExploring();
                break;

            default:
                // 其他對話（之後可以再加）
                GoExploring();
                break;
        }

        currentDialogueId = "";
    }

    // -------------------------------------------------
    // 共用：回到探索模式
    // -------------------------------------------------
    void GoExploring()
    {
        CurrentState = GameState.Exploring;

        if (playerMove)  playerMove.enabled  = true;
        if (playerFight) playerFight.enabled = false;
        if (enemiesRoot) enemiesRoot.SetActive(false);
    }

    // -------------------------------------------------
    // 練習場開始戰鬥
    // -------------------------------------------------
    void StartTrainingFight()
    {
        CurrentState = GameState.Fighting;
        killedEnemies = 0;

        if (playerMove)  playerMove.enabled  = false;
        if (playerFight) playerFight.enabled = true;
        if (enemiesRoot) enemiesRoot.SetActive(true); // 開啟數據蟲
    }

    // -------------------------------------------------
    // 敵人被殺死時（敵人腳本呼叫）
    // -------------------------------------------------
    public void OnEnemyKilled()
    {
        killedEnemies++;

        // 目前規則：殺死至少一隻就算通過
        if (CurrentStage == StoryStage.TrainingInProgress && killedEnemies >= 1)
        {
            // 關掉戰鬥，接對話三
            if (enemiesRoot) enemiesRoot.SetActive(false);
            if (playerFight) playerFight.enabled = false;

            // 開啟對話三
            StartStoryDialogue("TRAINING_END", "training_finish");
        }
    }
}
