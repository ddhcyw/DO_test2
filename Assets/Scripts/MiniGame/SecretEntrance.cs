using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SecretEntrance : MonoBehaviour
{
    [Header("UI 介面設定")]
    public GameObject puzzlePanel;       // 那個會跳出來的 Panel
    public Image leverImage;             // 拉桿原本的圖片 (UI Image)
    public Image leverDownSprite;       // 拉桿「拉下來」後的圖片

    [Header("場景設定")]
    public string baseSceneName = "作品集偷偷基地Scene"; 

    // =================================================
    // 1. 給「暗門物件」用的功能 (掛在場景物件上)
    // =================================================
    void OnMouseDown()
    {
        // 為了避免誤觸，檢查是否在對話中
        if (GameFlow.Instance != null && GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
            return;

        OpenPanel();
    }

    // 如果你的暗門是 UI 按鈕，也可以用這個
    public void OpenPanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }
    }

    // =================================================
    // 2. 給「正確拉桿 (UI Button)」用的功能
    // =================================================
    public void OnClickCorrectLever()
    {
        StartCoroutine(PullLeverProcess());
    }

    // 處理拉桿動畫與轉場的協程
    IEnumerator PullLeverProcess()
    {
        // 1. 換圖：變成拉下來的樣子
        if (leverImage != null && leverDownSprite != null)
        {
            leverImage.sprite = leverDownSprite.sprite;
            leverImage.SetNativeSize();
            leverImage.rectTransform.anchoredPosition += new Vector2(0, -105f);
        }

        // 2. 播放音效 (如果有設定的話，可在此加入)
        // AudioSource.PlayClipAtPoint(...);

        Debug.Log("拉桿已拉下！準備傳送...");

        // 3. 等待 0.5 ~ 1 秒，讓玩家看清楚拉桿動了
        yield return new WaitForSeconds(0.8f);

        // 4. 切換場景
        SceneManager.LoadScene(baseSceneName);
    }

    // (選用) 給錯誤拉桿用的，拉了只會晃一下或沒反應
    public void OnClickWrongLever()
    {
        Debug.Log("這個拉桿好像卡住了...");
        // 這裡可以做一點讓按鈕左右搖晃的動畫
    }
}