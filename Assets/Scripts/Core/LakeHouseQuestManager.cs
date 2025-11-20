using System.Collections;
using UnityEngine;

public class LakeHouseQuestManager : MonoBehaviour
{
    [Header("對話系統")]
    public DialogueController dialogue;
    public string maiKnotName = "lake_mai_after_investigate";

    int investigatedCount = 0;
    bool maiStarted = false;

    // 被 InteractZone 呼叫：某個調查點完成
    public void MarkInvestigated()
    {
        investigatedCount++;
        Debug.Log($"[LakeHouse] 已調查 {investigatedCount} 個");

        if (!maiStarted && investigatedCount >= 3)
        {
            maiStarted = true;
            StartCoroutine(StartMaiAfterCurrentDialogue());
        }
    }

    IEnumerator StartMaiAfterCurrentDialogue()
    {
        // 等目前對話結束
        while (dialogue != null && dialogue.IsPlaying)
            yield return null;

        if (dialogue != null)
            dialogue.StartInkDialogue(maiKnotName);
    }
}
