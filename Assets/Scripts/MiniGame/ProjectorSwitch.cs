using UnityEngine;

public class ProjectorSwitch : MonoBehaviour
{
    [Header("要消失的物件 (原本的投影畫面)")]
    public GameObject objectToHide;

    [Header("要出現的物件 (暗門/入口)")]
    public GameObject objectToShow;

    [Header("音效")]
    public AudioSource audioSource; // 如果有開關聲音可以掛上去

    // 場景中的物件靠 Collider 點擊
    void OnMouseDown()
    {
        // 檢查是否在對話中，避免誤觸
        if (GameFlow.Instance != null && GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
            return;

        PerformSwitch();
    }

    
    void PerformSwitch()
    {
        // 1. 讓原本的東西消失
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }

        // 2. 讓新的東西出現
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
        }

        // 3. 播放音效
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Debug.Log("投影機已關閉，暗門出現！");
    }
}