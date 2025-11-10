using UnityEngine;

public class GameFlow : MonoBehaviour
{
    // === 狀態定義 ===
    public enum GameState { Exploring, Talking, Fighting }
    public static GameState CurrentState { get; private set; } = GameState.Exploring;

    // === 欄位設定 ===
    [Header("角色控制")]
    public PlayerController playerMove;
    public PlayerControllerFight playerFight;

    [Header("對話系統")]
    public DialogueController dialogue;

    [Header("場景物件")]
    public GameObject mai;             // MAI 機器人
    public GameObject enemiesRoot;     // 數據蟲父物件（包含所有敵人）
    public GameObject maiHelpArea;     // MAI 幫助區域

    // === 狀態切換 ===
    public static void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"🎮 遊戲狀態切換為：{newState}");
    }

    void Start()
    {
        // 初始狀態：探索模式
        SwitchToExploring();
    }

    // 探索模式
    public void SwitchToExploring()
    {
        SetState(GameState.Exploring);

        if (playerMove) playerMove.enabled = true;
        if (playerFight) playerFight.enabled = false;

        if (dialogue) dialogue.root.SetActive(false);

        if (mai) mai.SetActive(true);
        if (maiHelpArea) maiHelpArea.SetActive(true);
        if (enemiesRoot) enemiesRoot.SetActive(false);
    }

    // 對話模式
    public void SwitchToTalking()
    {
        SetState(GameState.Talking);

        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = false;

        if (dialogue) dialogue.root.SetActive(true);
        if (maiHelpArea) maiHelpArea.SetActive(false);
    }

    // 戰鬥模式
    public void SwitchToFighting()
    {
        SetState(GameState.Fighting);

        if (playerMove) playerMove.enabled = false;
        if (playerFight) playerFight.enabled = true;

        if (dialogue) dialogue.root.SetActive(false);

        if (mai) mai.SetActive(false);
        if (maiHelpArea) maiHelpArea.SetActive(false);
        if (enemiesRoot) enemiesRoot.SetActive(true);
    }

    // === 供外部事件呼叫（例如 DialogueController） ===
    public void OnDialogueStarted()
    {
        SwitchToTalking();
    }

    public void OnDialogueFinished()
    {
        SwitchToFighting();
    }
}
