using UnityEngine;

public class MusicClickTrigger : MonoBehaviour
{
    [Header(" 請將剛剛寫好的 MusicFader 拖進來")]
    public MusicFader myMusicFader;

    [Header(" 你想要點擊來切換音樂的物件標籤 (Tag)")]
    public string targetTag = "Player";

    void Update()
    {
        // 偵測滑鼠左鍵點擊 (0 是左鍵)
        if (Input.GetMouseButtonDown(0))
        {
            CheckClick();
        }
    }

    void CheckClick()
    {

        // 將滑鼠螢幕座標轉換為遊戲世界座標
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 發射一道 2D 射線
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // 如果射線碰到了東西
        if (hit.collider != null)
        {
            // 檢查碰到的東西是不是我們要找的目標標籤
            if (hit.collider.CompareTag(targetTag))
            {
                Debug.Log($"<color=cyan>【點擊偵測】成功點擊到目標：{hit.collider.gameObject.name}！</color>");
                TriggerMusicChange();
            }
        }


    }

    void TriggerMusicChange()
    {
        // 呼叫 MusicFader 的終極切換按鈕
        if (myMusicFader != null)
        {
            myMusicFader.ForceSwitchToMainBGM();

            // 如果你只希望切換一次音樂，觸發後可以把這個偵測腳本關掉，節省效能
            this.enabled = false;
        }
        else
        {
            Debug.LogError("【錯誤】MusicClickTrigger 找不到 MusicFader，請檢查 Inspector 有沒有拖拉！");
        }
    }
}