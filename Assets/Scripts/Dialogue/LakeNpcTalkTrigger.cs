using UnityEngine;

public class LakeNpcTalkTrigger : MonoBehaviour
{
    public string knotName;     // Ink 節點名稱，例如 "lake_npc1"、"lake_npc2"
    public GameFlow gameFlow;   // 指到 SceneManager 上的 GameFlow

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (gameFlow == null) return;

        // 避免正在對話時又觸發
        if (gameFlow.CurrentState == GameFlow.GameState.Exploring)
        {
            gameFlow.StartDialogue(knotName);
        }
    }
}
