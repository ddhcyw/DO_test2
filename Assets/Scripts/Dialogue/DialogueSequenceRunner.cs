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
        if (dialogue) dialogue.pauseRequested = true;
        StartCoroutine(CoPauseDialogue(seconds));
    }

    IEnumerator CoPauseDialogue(float seconds)
    {
        running = true;

        // 1. 關對話框
        if (dialogue) dialogue.TempHide();

        // 2. 讓場景 MAI 消失（橋邊）
        if (gameFlow) gameFlow.HideMai("bridge");

        // 3. 等待
        yield return new WaitForSeconds(seconds);

        // 4. 隱藏幫助區，再回到對話繼續
        if (gameFlow) gameFlow.HideMAIHelp();
        if (dialogue) dialogue.TempShowAndContinue();

        running = false;
    }

}
