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

    private bool triggered = false;

    void OnMouseDown()
    {
        if (triggered) return;
        if (!dialogue)
        {
            Debug.LogError($"{name}: dialogue 沒有指定！");
            return;
        }
        if (dialogue.IsPlaying) return;

        if (player != null)
        {
            float d = Vector2.Distance(player.position, transform.position);
            if (d > interactDistance) return;
        }

        if (dimmerObject != null) dimmerObject.SetActive(false);

        triggered = true;
        dialogue.StartInkDialogue(inkKnotName);
    }
}