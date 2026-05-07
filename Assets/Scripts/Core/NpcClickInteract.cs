using UnityEngine;
using Game.Dialogue;

[RequireComponent(typeof(Collider2D))]
public class NpcClickInteract : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueController dialogue;

    [Header("Ink")]
    public string inkKnotName;

    [Header("Distance Gate")]
    public Transform player;
    public float interactDistance = 3.0f;

    [Header("Optional")]
    public GameObject dimmerObject;

    [Header("Behavior")]
    public bool triggerOnce = false;

    [Header("Repeat Dialogue")]
    public string repeatKnotName;
    [HideInInspector] public bool dialogueCompleted = false;

    [Header("Quest Flag（填入 PlayerPrefs key，完成時自動切換到 repeatKnotName）")]
    public string questCompletedFlag;

    private bool triggered = false;

    void Start()
    {
        if (!string.IsNullOrEmpty(questCompletedFlag))
            if (PlayerPrefs.GetInt(questCompletedFlag, 0) == 1)
                dialogueCompleted = true;
    }

    void OnMouseDown()
    {
        Debug.Log($"OnMouseDown hit: {name}");

        if (!dialogue)
        {
            Debug.LogError($"{name}: dialogue 沒有指定！");
            return;
        }
        if (dialogue.IsPlaying) return;

        if (player != null)
        {
            Vector2 p = player.position;
            Vector2 n = transform.position;
            float d = Vector2.Distance(p, n);
            Debug.Log($"Distance to player = {d}, gate = {interactDistance}");

            if (d > interactDistance) return;
        }

        if (dimmerObject != null) dimmerObject.SetActive(false);

        if (dialogueCompleted && !string.IsNullOrEmpty(repeatKnotName))
        {
            dialogue.StartInkDialogue(repeatKnotName);
        }
        else
        {
            if (triggerOnce && triggered) return;
            if (triggerOnce) triggered = true;
            dialogue.StartInkDialogue(inkKnotName);
            if (!string.IsNullOrEmpty(repeatKnotName) && GetComponent<NpcInteraction>() == null)
                dialogueCompleted = true;
        }
    }
}