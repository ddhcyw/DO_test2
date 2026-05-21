using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportDoor : MonoBehaviour
{
    [Header("傳送設定")]
    public string targetSceneName;

    [Header("鎖定狀態")]
    public bool isUnlocked = false;

    private Collider2D doorCollider;
    private bool isTransitioning = false;

    void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isUnlocked || isTransitioning) return;

        if (doorCollider == null)
        {
            Debug.LogError("[TeleportDoor] doorCollider 是 null！");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[TeleportDoor] 找不到 Tag=Player 的物件！");
            return;
        }

        bool playerInZone = doorCollider.OverlapPoint(player.transform.position);

        if (playerInZone && Input.GetMouseButtonDown(0))
        {
            var stm = SceneTransitionManager.Instance;
            if (stm == null) stm = FindObjectOfType<SceneTransitionManager>();

            if (stm != null && stm.IsTransitioning) return;

            isTransitioning = true;
            Debug.Log($"[TeleportDoor] 準備傳送至：{targetSceneName}");

            if (stm != null)
                stm.TransitionToScene(targetSceneName);
            else
            {
                Debug.LogWarning("[TeleportDoor] SceneTransitionManager 找不到，直接跳場");
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("傳送門已解鎖！");
    }
}
