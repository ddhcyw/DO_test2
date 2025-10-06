using UnityEngine;

public class DialogueBoot : MonoBehaviour
{
    public DialogueController controller;
    void Start()
    {
        controller.StartDialogue("intro");
    }
}
