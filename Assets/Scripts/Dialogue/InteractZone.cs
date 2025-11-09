using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractZone : MonoBehaviour
{
    public GameFlow gameFlow;       // 由 Inspector 指定 SceneManager
    public GameObject pressEHint;   // 「按 E 對話」提示 UI
    private bool playerInside = false;

    void Start()
    {
        // 確保提示一開始是隱藏的
        if (pressEHint) pressEHint.SetActive(false);
    }

    void Update()
    {
        // 只有在探索模式，且玩家在範圍內，才可以按 E 觸發對話
        if (playerInside &&
            GameFlow.CurrentState == GameFlow.GameState.Exploring &&
            Input.GetKeyDown(KeyCode.E))
        {
            if (pressEHint) pressEHint.SetActive(false);
            if (gameFlow) gameFlow.SwitchToTalking();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if (pressEHint) pressEHint.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (pressEHint) pressEHint.SetActive(false);
        }
    }
}
