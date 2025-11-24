using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractZone : MonoBehaviour
{
    [Header("對話系統")]
    // 指到場景中 DialogueSystemRoot 上的 DialogueController
    public DialogueController dialogue;

    [Header("Ink 設定")]
    // 這個區域要播放的 Ink knot 名稱（例如 bridge_intro、rocket_scene、training_finish）
    public string inkKnotName = "bridge_intro";

    public GameObject dimmerObject;

    bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (!dialogue)
        {
            Debug.LogError("InteractZone: dialogue 沒有指定！");
            return;
        }
        if (dimmerObject != null)
        {
            dimmerObject.SetActive(false); 
        }

        triggered = true;

        // 呼叫 DialogueController 的 Ink 版本
        dialogue.StartInkDialogue(inkKnotName);
    }
}
