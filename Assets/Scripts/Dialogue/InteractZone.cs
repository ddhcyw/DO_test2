using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractZone : MonoBehaviour
{
    public DialogueController dialogue;   // 指向 DialogueSystemRoot 上的 DialogueController
    bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        if (dialogue)
        {
            dialogue.StartDialogue();
        }
        else
        {
            Debug.LogError("InteractZone: dialogue 沒有指定！");
        }
    }
}
