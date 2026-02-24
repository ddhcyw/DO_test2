using UnityEngine;
using UnityEngine.SceneManagement; // 切換場景必備的函式庫

public class TeleportDoor : MonoBehaviour
{
    [Header("傳送設定")]
    public string targetSceneName; // 目的地場景的名稱

    [Header("狀態設定")]
    public bool isUnlocked = false; // 預設為 false (鎖住狀態)

    // 這個函式準備給「對話系統」在對話結束時呼叫
    public void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("傳送門已解鎖！");
    }

    // 當玩家走進門的碰撞範圍時觸發
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 檢查碰到門的是不是玩家
        if (collision.CompareTag("Player"))
        {
            if (isUnlocked)
            {
                Debug.Log($"準備傳送到：{targetSceneName}");
                SceneManager.LoadScene(targetSceneName); // 載入新場景
            }
            else
            {
                Debug.Log("門目前是鎖上的！");
            }
        }
    }
}