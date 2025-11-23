using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow; // 用來呼叫外部指令
    public bool IsPlaying => panelRoot != null && panelRoot.activeSelf;

    [Header("UI Components")]
    public GameObject panelRoot;    // 對話面板 (DialoguePanel)
    public TMP_Text nameText;       // 顯示名字的 Text
    public TMP_Text bodyText;       // 顯示內容的 Text
    public GameObject continueHint; // 繼續對話的小箭頭/提示

    [Header("Typing Settings")]
    public bool typewriter = true;      // 是否開啟打字機效果
    public float charsPerSecond = 40f;  // 打字速度

    [Header("Ink Data")]
    public TextAsset inkJSONAsset; // Ink 編譯後的 .json 檔案

    // 內部變數
    Story inkStory;
    Coroutine typingCo;
    string currentLineText = "";

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);
    }

    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (typingCo != null)
            {
                StopCoroutine(typingCo);
                typingCo = null;
                if (bodyText) bodyText.text = currentLineText;
                if (continueHint) continueHint.SetActive(true);
            }
            else
            {
                ContinueInk();
            }
        }
    }

    // 啟動對話
    public void StartInkDialogue(string knotName)
    {
        Debug.Log($"🟦 DialogueController: 啟動對話節點 '{knotName}'");

        if (!panelRoot || !inkJSONAsset)
        {
            Debug.LogError("DialogueController: 缺少 panelRoot 或 inkJSONAsset 設定！");
            return;
        }

        inkStory = new Story(inkJSONAsset.text);
        inkStory.allowExternalFunctionFallbacks = true;

        BindExternal();

        if (!string.IsNullOrEmpty(knotName))
        {
            try
            {
                inkStory.ChoosePathString(knotName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DialogueController: 找不到 Ink 節點 '{knotName}'。錯誤: {e.Message}");
            }
        }

        panelRoot.SetActive(true);
        if (continueHint) continueHint.SetActive(false);
        if (nameText) nameText.text = "";
        if (bodyText) bodyText.text = "";

        if (gameFlow) gameFlow.OnDialogueStarted();

        ContinueInk();
    }

    // 綁定外部函數
    void BindExternal()
    {
        if (inkStory == null)
        {
            Debug.LogError("DialogueController: inkStory 還沒建立就嘗試綁定 external");
            return;
        }

        inkStory.BindExternalFunction("give_camera", () =>
        {
            if (gameFlow != null)
                gameFlow.GiveCamera();
            else
                Debug.LogWarning("give_camera 被呼叫，但場景中沒有 GameFlow。");
        });
        inkStory.BindExternalFunction("get_camera_item", () => gameFlow.GetCameraItem());

        inkStory.BindExternalFunction("show_objective", (string content) => {
            gameFlow.ShowObjectiveUI(content);
        });

        inkStory.BindExternalFunction("spawn_wave", () =>
        {
            if (gameFlow != null)
                gameFlow.SetSpawnTrainingBugAfterDialogue();
            else
                Debug.LogWarning("spawn_wave 被呼叫，但場景中沒有 GameFlow。");
        });

        // 圖像廣場
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());
    }

    // 讀取下一句對話（已加入「跳過空行」邏輯）
    void ContinueInk()
    {
        if (inkStory == null)
        {
            EndDialogue();
            return;
        }

        // 連續吃掉「只有指令、沒有文字」的步驟
        string line = null;

        while (inkStory.canContinue && string.IsNullOrWhiteSpace(line))
        {
            line = inkStory.Continue();
            if (line != null)
                line = line.Trim();
        }

        // 如果已經沒有文字可以顯示，就結束對話
        if (string.IsNullOrWhiteSpace(line))
        {
            EndDialogue();
            return;
        }

        string who = "";
        string text = line;

        int colonIndex = line.IndexOf(':');
        if (colonIndex > 0)
        {
            who = line.Substring(0, colonIndex).Trim();
            text = line.Substring(colonIndex + 1).Trim();
        }
        else if ((colonIndex = line.IndexOf('：')) > 0)
        {
            who = line.Substring(0, colonIndex).Trim();
            text = line.Substring(colonIndex + 1).Trim();
        }

        currentLineText = text;

        if (nameText) nameText.text = who;
        if (bodyText) bodyText.text = "";

        if (typewriter)
        {
            if (continueHint) continueHint.SetActive(false);
            if (typingCo != null) StopCoroutine(typingCo);
            typingCo = StartCoroutine(TypeText(text));
        }
        else
        {
            if (bodyText) bodyText.text = text;
            if (continueHint) continueHint.SetActive(true);
        }
    }

    IEnumerator TypeText(string content)
    {
        float t = 0;
        int charIndex = 0;
        int totalChars = content.Length;

        while (charIndex < totalChars)
        {
            t += Time.deltaTime * Mathf.Max(1, charsPerSecond);
            charIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, totalChars);

            if (bodyText)
                bodyText.text = content.Substring(0, charIndex);

            yield return null;
        }

        if (bodyText) bodyText.text = content;
        typingCo = null;
        if (continueHint) continueHint.SetActive(true);
    }

    void EndDialogue()
    {
        Debug.Log("🟥 DialogueController: 對話結束");

        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);

        inkStory = null;

        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}
