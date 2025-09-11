using System.Collections;
using System.Collections.Generic; // 用於 List
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

#region Data Structures for API
// 用於序列化單條對話訊息
[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;

    public ChatMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

// 用於序列化要發送到伺服器的對話列表
[System.Serializable]
public class ChatPayload
{
    public List<ChatMessage> messages;
}

// 用於解析本地 Flask (Qwen) 伺服器回應的資料結構
[System.Serializable]
public class QwenResponse
{
    public string response;
}
#endregion

public class AskMAI_API : MonoBehaviour
{
    [Header("伺服器設定")]
    [Tooltip("本地 Flask 伺服器的網址")]
    public string localURL = "http://127.0.0.1:8000/ask";
    

    [Header("UI 元件")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField inputField;
    public Button sendButton;
    [Tooltip("可選：對話內容的滾動視圖，用於顯示完整歷史")]
    public ScrollRect dialogueScrollView;

    // 用於儲存對話歷史
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    void Start()
    {
        if (nameText != null) nameText.text = "MAI";
        sendButton?.onClick.AddListener(OnSend);
        if (inputField != null)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }

        // --- 關鍵修改點 ---
        // 1. 建立初始的問候語訊息
        ChatMessage initialMessage = new ChatMessage("assistant", "你好！我是 MAI，有什麼可以幫你的嗎？");

        // 2. 將這句問候語加入到對話歷史中進行追蹤
        conversationHistory.Add(initialMessage);

        // 3. 呼叫統一的更新函式來顯示內容
        UpdateDialogueDisplay();
    }

    void Update()
    {
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnSend();
        }
    }

    public void OnSend()
    {
        string userInput = inputField.text.Trim();
        if (string.IsNullOrEmpty(userInput) || !sendButton.interactable) return;

        // 將使用者的輸入加入歷史紀錄
        conversationHistory.Add(new ChatMessage("user", userInput));
        UpdateDialogueDisplay(); // 更新畫面顯示

        // 清空輸入框並禁用按鈕，防止重複發送
        inputField.text = "";
        SetInteraction(false);

        // ** 關鍵修改 **
        // 將整個對話歷史傳送到伺服器
        StartCoroutine(SendRequestToServer(conversationHistory));
    }

    IEnumerator SendRequestToServer(List<ChatMessage> history)
    {
        string apiURL = localURL; // 此功能僅針對本地伺服器優化

        // --- 構建 JSON Payload ---
        // 伺服器現在期望的格式: {"messages": [{"role": "user", "content": "..."}, ...]}
        ChatPayload payload = new ChatPayload { messages = history };
        string jsonPayload = JsonUtility.ToJson(payload);

        // --- 建立並設定 UnityWebRequest ---
        UnityWebRequest req = new UnityWebRequest(apiURL, "POST");
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        // --- 處理回應 ---
        if (req.result == UnityWebRequest.Result.Success)
        {
            string rawResponse = req.downloadHandler.text;
            string reply = ParseLocalResponse(rawResponse);

            // 將 MAI 的回覆加入歷史紀錄
            conversationHistory.Add(new ChatMessage("assistant", reply));
            UpdateDialogueDisplay(); // 更新畫面顯示
        }
        else
        {
            dialogueText.text += "\n<color=red>MAI 回應失敗...</color>";
            Debug.LogError($"API 請求錯誤: {req.error}\n回應內容: {req.downloadHandler.text}");
        }

        // 重新啟用 UI
        SetInteraction(true);
    }

    /// <summary>
    /// 更新對話框中顯示的文字
    /// </summary>
    private void UpdateDialogueDisplay()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var message in conversationHistory)
        {
            if (message.role == "user")
            {
                sb.AppendLine($"<b><color=#2E86C1>你：</color></b>{message.content}");
            }
            else
            {
                sb.AppendLine($"<b><color=#AF7AC5>MAI：</color></b>{message.content}");
            }
            sb.AppendLine(); // 增加間距
        }
        dialogueText.text = sb.ToString();

        // 如果有設定 ScrollView，自動滾動到底部
        StartCoroutine(ForceScrollDown());
    }

    IEnumerator ForceScrollDown()
    {
        // 等待一幀讓 UI 更新
        yield return new WaitForEndOfFrame();
        if (dialogueScrollView != null)
        {
            dialogueScrollView.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 控制 UI 互動性
    /// </summary>
    private void SetInteraction(bool isInteractable)
    {
        sendButton.interactable = isInteractable;
        inputField.interactable = isInteractable;
        if (isInteractable)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    /// <summary>
    /// 解析來自本地 Flask 伺服器的 JSON 回應
    /// </summary>
    string ParseLocalResponse(string json)
    {
        try
        {
            QwenResponse res = JsonUtility.FromJson<QwenResponse>(json);
            return res.response.Trim();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("解析本地回應失敗: " + ex.Message + "\n收到的 JSON: " + json);
            return "(無法解析 MAI 本地回應)";
        }
    }
}
