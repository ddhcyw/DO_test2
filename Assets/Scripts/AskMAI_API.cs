using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AskMAI_API : MonoBehaviour
{
    [Header("伺服器設定")]
    public bool useLocalServer = false;
    public bool isDemoMode = false;

    [Header("API 設定")]
    public string localURL = "http://127.0.0.1:8000/ask";
    public string huggingfaceURL = "https://api-inference.huggingface.co/models/deepseek-ai/DeepSeek-V3";
    public string huggingfaceToken = "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";

    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField inputField;
    public Button sendButton;

    void Start()
    {
        if (nameText != null)
            nameText.text = "MAI";

        sendButton?.onClick.AddListener(() => Ask(inputField.text));
    }

    void Update()
    {
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                Ask(inputField.text);
            }
        }
    }

    public void Ask(string playerText)
    {
        if (string.IsNullOrWhiteSpace(playerText)) return;

        inputField.text = "";
        dialogueText.text = "思考中...";

        if (isDemoMode)
        {
            string response = GetDemoResponse(playerText);
            dialogueText.text = "MAI：" + response;
            return;
        }

        string prompt = "你是一位親切的 AI 助手 MAI，請用繁體中文回答：" + playerText;
        Debug.Log("送出的 prompt：\n" + prompt);
        StartCoroutine(SendToServer(prompt));
    }

    private string GetDemoResponse(string input)
    {
        input = input.Trim().ToLower();

        if (Regex.IsMatch(input, "你是誰"))
        {
            return "我是這個網路城的嚮導，看你這副樣子......是從現實世界來的吧";
        }
        else if (Regex.IsMatch(input, "怎麼回家"))
        {
            return "你可以搭乘飛船「英特涅號」回去，但不幸的是它現在能量不足";
        }
        else if (Regex.IsMatch(input, "怎麼辦"))
        {
            return "英特涅城最近不太安寧，必須淨化搗亂的隱患才能獲取飛船能量，別擔心!我會全力幫助你的";
        }
        else
        {
            return "抱歉，我不太明白你的問題。";
        }
    }

    IEnumerator SendToServer(string prompt)
    {
        string apiURL = useLocalServer ? localURL : huggingfaceURL;
        string escapedPrompt = prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        string json = "{\"inputs\": \"" + escapedPrompt + "\"}";

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(apiURL, "POST");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (!useLocalServer && !string.IsNullOrEmpty(huggingfaceToken))
            req.SetRequestHeader("Authorization", "Bearer " + huggingfaceToken);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string res = req.downloadHandler.text;
            string answer = ExtractPlainText(res);
            dialogueText.text = "MAI：" + Regex.Unescape(answer);
            Debug.Log("MAI 回覆：" + answer);
        }
        else
        {
            dialogueText.text = "MAI 回應失敗（" + req.responseCode + ")";
            Debug.LogError("API 請求失敗: " + req.error);
        }

        inputField.ActivateInputField();
    }

    string ExtractPlainText(string json)
    {
        int i = json.IndexOf(":") + 2;
        int j = json.LastIndexOf("\"");
        if (i < 2 || j < i) return "無法解析 MAI 回覆";
        return json.Substring(i, j - i);
    }
}
