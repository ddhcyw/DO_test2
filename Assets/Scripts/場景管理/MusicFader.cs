using UnityEngine;
using System.Collections;

public class MusicFader : MonoBehaviour
{
    [Header(" 喇叭 A (專門播放：第二首 主背景音樂)")]
    public AudioSource mainBGMSource;
    public float fadeDuration = 3.0f;
    public float targetVolume = 0.5f;

    [Header(" 喇叭 B (專門播放：第一首 故事開頭歌)")]
    public AudioSource openingSource;
    public float openingVolume = 0.5f;

    void Start()
    {
        // 遊戲一開始，只單純播放開頭音樂 (不負責倒數切換)
        if (openingSource != null && openingSource.clip != null)
        {
            openingSource.volume = openingVolume;
            openingSource.loop = true; // 建議設為循環，直到玩家去點擊角色
            openingSource.Play();
        }

        // 確保主音樂是安靜的
        if (mainBGMSource != null)
        {
            mainBGMSource.volume = 0;
            mainBGMSource.Stop();
        }
    }
    // 加回這個保險接口，讓 GameFlow 和 DialogueController 不會報錯
    public void StartFadeIn()
    {
        ForceSwitchToMainBGM();
    }

    //  給外部 (點擊角色、對話系統) 呼叫的終極切換按鈕
    public void ForceSwitchToMainBGM()
    {
        Debug.Log("<color=yellow>【手動觸發】玩家點擊！強制切換至主背景音樂</color>");

        // 1. 停止所有正在進行的協程 (避免重複觸發)
        StopAllCoroutines();

        // 2. 乾淨俐落地關掉開頭音樂
        if (openingSource != null && openingSource.isPlaying)
        {
            openingSource.Stop();
        }

        // 3. 啟動淡入主音樂的協程
        StartCoroutine(FadeInMainRoutine());
    }

    IEnumerator FadeInMainRoutine()
    {
        float timer = 0f;

        mainBGMSource.volume = 0;
        mainBGMSource.loop = true;
        mainBGMSource.Play();

        while (timer < fadeDuration)
        {
            // 使用不受暫停影響的時間
            timer += Time.unscaledDeltaTime;
            mainBGMSource.volume = Mathf.Lerp(0, targetVolume, timer / fadeDuration);
            yield return null;
        }

        mainBGMSource.volume = targetVolume;
        Debug.Log($"<color=lime>【物理隔離音樂系統】主背景音樂淡入完成！</color>");
    }
}