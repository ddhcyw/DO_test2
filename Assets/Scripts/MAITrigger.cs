using UnityEngine;

public class MAITrigger : MonoBehaviour
{
    public GameObject dialoguePanel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialoguePanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialoguePanel.SetActive(false);
        }
    }
}
