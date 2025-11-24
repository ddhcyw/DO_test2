using UnityEngine;
using System.Collections; // 必須引用這個，因為要用協程

public class MusicFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 3.0f; // 淡入需要幾秒
    public float targetVolume = 0.5f; // 目標音量 (0~1)

    void Start()
    {
        // 如果沒有手動指定 AudioSource，就抓取自己身上的
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 1. 先把音量設為 0
        audioSource.volume = 0;

        // 2. 開始播放音樂 (如果還沒播的話)
        if (!audioSource.isPlaying)
            audioSource.Play();

        // 3. 啟動淡入功能的協程
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime; // 累加經過的時間

            // Mathf.Lerp 是數學插值，幫你算出 0 到 目標音量 之間的中間值
            // timer / fadeDuration 是一個 0 到 1 的進度比例
            audioSource.volume = Mathf.Lerp(0, targetVolume, timer / fadeDuration);

            yield return null; // 等待下一幀再繼續執行
        }

        // 確保最後音量精準設定為目標值
        audioSource.volume = targetVolume;
    }
}