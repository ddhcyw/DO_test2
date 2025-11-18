using UnityEngine;

public class TalkTrigger : MonoBehaviour
{
    public string dialogueId;   // 例如 "MAI1", "MAI2"
    public string inkKnotName;  // Ink 裡面對應的 knot 名

    bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        // 只有在 Exploring 時才觸發
        if (GameFlow.Instance.CurrentState != GameFlow.GameState.Exploring)
            return;

        // 限制劇情順序
        var stage = GameFlow.Instance.CurrentStage;
        if (dialogueId == "MAI1" && stage != GameFlow.StoryStage.None)
            return;

        if (dialogueId == "MAI2" && stage != GameFlow.StoryStage.MetMai1)
            return;

        used = true;

        GameFlow.Instance.StartStoryDialogue(dialogueId, inkKnotName);
    }
}
