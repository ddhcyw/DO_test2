using UnityEngine;
using System.Collections; // 必須引用這個，因為要用協程

public class MusicFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 3.0f; // 淡入需要幾秒
    public float targetVolume = 0.5f; // 目標音量 (0~1)

    public void StartFadeIn()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 保險檢查：如果還是沒有 AudioSource 就跳過
        if (audioSource == null) return;

        // 如果音樂已經在播了，就不要重複觸發淡入，否則音量會突然變回 0
        if (audioSource.isPlaying) return;

        audioSource.volume = 0;
        audioSource.Play();
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