using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractZone : MonoBehaviour
{
    public GameFlow gameFlow;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameFlow.EnableTalk(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameFlow.EnableTalk(false);
        }
    }
}
