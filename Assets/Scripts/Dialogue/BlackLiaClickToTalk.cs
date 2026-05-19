using UnityEngine;
using UnityEngine.EventSystems;

public class BlackLiaClickToTalk : MonoBehaviour
{
    public GameFlow gameFlow;

    public string knotBeforeClue = "talk_blacklia_before_clue";
    public string knotAfterClue  = "talk_blacklia_after_clue";

    void Reset()
    {
        gameFlow = GameFlow.Instance;
    }

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (gameFlow == null) gameFlow = GameFlow.Instance;
        if (gameFlow == null) return;

        // 對話進行中或戰鬥中不插隊
        if (gameFlow.CurrentState != GameFlow.GameState.Exploring) return;

        var dc = DialogueController.Instance;
        if (dc != null && dc.IsPlaying) return;

        bool all = gameFlow.HasAllBaseClues();
        gameFlow.StartDialogue(all ? knotAfterClue : knotBeforeClue);
    }
}
