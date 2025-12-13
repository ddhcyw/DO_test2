using UnityEngine;

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
        if (gameFlow == null) gameFlow = GameFlow.Instance;
        if (gameFlow == null) return;

        bool all = gameFlow.HasAllBaseClues();
        gameFlow.StartDialogue(all ? knotAfterClue : knotBeforeClue);
    }
}
