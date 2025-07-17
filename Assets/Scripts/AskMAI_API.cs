using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AskMAI_API : MonoBehaviour
{
    [Header("���A���]�w")]
    public bool useLocalServer = false;
    public bool isDemoMode = false;

    [Header("API �]�w")]
    public string localURL = "http://127.0.0.1:8000/ask";
    public string huggingfaceURL = "https://api-inference.huggingface.co/models/deepseek-ai/DeepSeek-V3";
    public string huggingfaceToken = "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";

    [Header("UI ����")]
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
        dialogueText.text = "��Ҥ�...";

        if (isDemoMode)
        {
            string response = GetDemoResponse(playerText);
            dialogueText.text = "MAI�G" + response;
            return;
        }

        string prompt = "�A�O�@��ˤ��� AI �U�� MAI�A�Х��c�餤��^���G" + playerText;
        Debug.Log("�e�X�� prompt�G\n" + prompt);
        StartCoroutine(SendToServer(prompt));
    }

    private string GetDemoResponse(string input)
    {
        input = input.Trim().ToLower();

        if (Regex.IsMatch(input, "�A�O��"))
        {
            return "�ڬO�o�Ӻ��������Q�ɡA�ݧA�o�Ƽˤl......�O�q�{��@�ɨӪ��a";
        }
        else if (Regex.IsMatch(input, "���^�a"))
        {
            return "�A�i�H�f������u�^�S�I���v�^�h�A���������O���{�b��q����";
        }
        else if (Regex.IsMatch(input, "����"))
        {
            return "�^�S�I���̪񤣤Ӧw��A�����b�Ʒo�ê����w�~����������q�A�O���!�ڷ|���O���U�A��";
        }
        else
        {
            return "��p�A�ڤ��ө��էA�����D�C";
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
            dialogueText.text = "MAI�G" + Regex.Unescape(answer);
            Debug.Log("MAI �^�СG" + answer);
        }
        else
        {
            dialogueText.text = "MAI �^�����ѡ]" + req.responseCode + ")";
            Debug.LogError("API �ШD����: " + req.error);
        }

        inputField.ActivateInputField();
    }

    string ExtractPlainText(string json)
    {
        int i = json.IndexOf(":") + 2;
        int j = json.LastIndexOf("\"");
        if (i < 2 || j < i) return "�L�k�ѪR MAI �^��";
        return json.Substring(i, j - i);
    }
}
