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
    public GameObject maiHelpArea;   // MAI 幫助區
    public GameObject enemiesRoot;   // 數據蟲父物件（預設 Hidden）

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

    // ================= 對話結束 =================
    public void OnDialogueFinished()
    {
        Debug.Log("🟥 GameFlow.OnDialogueFinished()");

        if (enemiesRoot) enemiesRoot.SetActive(true);

        CurrentState = GameState.Fighting;

        if (playerFight) playerFight.enabled = true;
    }
}
