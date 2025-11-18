using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TalkTrigger : MonoBehaviour
{
    [Header("對話系統")]
    // 指向場景裡的 DialogueController（通常在 DialogueSystemRoot 上）
    public DialogueController dialogue;

    [Header("Ink 設定")]
    // 要播放的 Ink knot 名稱（例如 bridge_intro / rocket_scene / training_finish）
    public string inkKnotName = "bridge_intro";

    [Tooltip("這個對話結束後是否要生出練習用 Databug")]
    public bool spawnTrainingBugAfterDialogue = false;

    bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (!dialogue)
        {
            Debug.LogError("TalkTrigger: DialogueController 沒有指定！");
            return;
        }

        // 如果有 GameFlow，而且目前不是在探索狀態，就先不要插隊開對話
        if (GameFlow.Instance &&
            GameFlow.Instance.CurrentState != GameFlow.GameState.Exploring)
        {
            return;
        }

        triggered = true;

        // 如果這個對話結束後要生 Databug，就先通知 GameFlow
        if (spawnTrainingBugAfterDialogue && GameFlow.Instance)
        {
            GameFlow.Instance.SetSpawnTrainingBugAfterDialogue();
        }

        // 開啟指定 knot 的 Ink 對話
        dialogue.StartInkDialogue(inkKnotName);
    }
}
