using UnityEngine;
using System.Collections;

public class DialogueSequenceRunner : MonoBehaviour
{
    public DialogueController dialogue;
    public GameFlow gameFlow;

    bool running = false;

    public void PauseDialogue(float seconds)
    {
        if (running) return;
        StartCoroutine(CoPauseDialogue(seconds));
    }

    IEnumerator CoPauseDialogue(float seconds)
    {
        running = true;

        // 1. 關對話框
        if (dialogue) dialogue.TempHide();

        // 2. 讓場景 MAI 消失（橋邊）
        if (gameFlow) gameFlow.HideMai("bridge");

        // 3. 開右下角 MAI 幫助區
        if (gameFlow) gameFlow.StartMAIHelp();

        // 4. 等待
        yield return new WaitForSeconds(seconds);

        // 5. 回到對話繼續
        if (dialogue) dialogue.TempShowAndContinue();

        running = false;
    }

}
