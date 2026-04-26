using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    [Header("傳送設定")]
    public string targetSceneName;

    [Header("鎖定狀態")]
    public bool isUnlocked = false;

    public void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("傳送門已解鎖！");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (isUnlocked)
        {
            Debug.Log($"準備傳送至：{targetSceneName}");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else
        {
            Debug.Log("此傳送門尚未解鎖！");
        }
    }
}
