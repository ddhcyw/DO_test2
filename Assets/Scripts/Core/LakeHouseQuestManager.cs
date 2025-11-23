using System.Collections;
using UnityEngine;

public class LakeHouseQuestManager : MonoBehaviour
{
    [Header("對話系統")]
    [Tooltip("拖 DialogueSystemRoot 上的 DialogueController 進來")]
    public DialogueController dialogue;

    [Header("設定")]
    [Tooltip("三個調查點都看完後要播放的 Ink knot 名稱")]
    public string maiKnotName = "lake_mai_after_investigate";

    int investigatedCount = 0;   // 已完成的調查點數量
    bool maiStarted = false;     // 是否已經觸發過 MAI 對話

    // 由 InteractZone / 事件呼叫：某個調查點完成一次
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
        // 等目前那一段調查對話播完
        while (dialogue != null && dialogue.IsPlaying)
            yield return null;

        if (dialogue != null && !string.IsNullOrEmpty(maiKnotName))
        {
            Debug.Log("[LakeHouse] 三個調查完成，啟動 MAI 對話");
            dialogue.StartInkDialogue(maiKnotName);
        }
    }
}
