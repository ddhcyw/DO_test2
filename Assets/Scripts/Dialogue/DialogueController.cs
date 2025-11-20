using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime; 
public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow; // 用來呼叫外部指令

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
    private Story inkStory;
    private Coroutine typingCo;
    private string currentLineText = "";

    void Awake()
    {
        // 遊戲開始時隱藏對話框
        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);
    }

    void Update()
    {
        // 如果面板沒開，就不偵測輸入
        if (!panelRoot || !panelRoot.activeSelf) return;

        // 偵測滑鼠左鍵或空白鍵
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // 情況 1: 正在打字中 -> 瞬間顯示全句
            if (typingCo != null)
            {
                StopCoroutine(typingCo);
                typingCo = null;
                if (bodyText) bodyText.text = currentLineText;
                if (continueHint) continueHint.SetActive(true);
            }
            // 情況 2: 已經顯示完畢 -> 下一句
            else
            {
                ContinueInk();
            }
        }
    }

    // ============================================================
    //  啟動對話 (由 GameFlow 或 InteractZone 呼叫)
    // ============================================================
    public void StartInkDialogue(string knotName)
    {
        Debug.Log($"🟦 DialogueController: 啟動對話節點 '{knotName}'");

        if (!panelRoot || !inkJSONAsset)
        {
            Debug.LogError("DialogueController: 缺少 panelRoot 或 inkJSONAsset 設定！");
            return;
        }

        // 1. 建立新的 Story 實例
        inkStory = new Story(inkJSONAsset.text);

        // 2. 綁定外部函數 (這是與 GameFlow 溝通的關鍵)
        BindExternal();

        // 3. 跳轉到指定的節點 (Knot)
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

        // 4. 開啟 UI 並通知 GameFlow
        panelRoot.SetActive(true);
        if (continueHint) continueHint.SetActive(false);
        if (nameText) nameText.text = "";
        if (bodyText) bodyText.text = "";

        if (gameFlow) gameFlow.OnDialogueStarted();

        // 5. 開始顯示第一句
        ContinueInk();
    }

    // ============================================================
    //  綁定外部函數 (Ink -> C#)
    // ============================================================
    void BindExternal()
    {
        if (gameFlow == null)
        {
            Debug.LogError("DialogueController: GameFlow 未指定，無法綁定外部函數！");
            return;
        }

        // 綁定 GDD 需求的指令：

        // 1. 給予相機 (~ give_camera())
        inkStory.BindExternalFunction("give_camera", () => {
            gameFlow.GiveCamera();
        });

        // 2. 顯示任務指示 (~ show_objective("目標", "提示"))
        inkStory.BindExternalFunction("show_objective", (string target, string hint) => {
            gameFlow.ShowObjectiveUI(target, hint);
        });

        // 3. 產生怪物波次 / 設置戰鬥 (~ spawn_wave())
        inkStory.BindExternalFunction("spawn_wave", () => {
            gameFlow.SetSpawnTrainingBugAfterDialogue();
        });
    }

    // ============================================================
    //  讀取下一句對話
    // ============================================================
    void ContinueInk()
    {
        // 如果故事沒了，或無法繼續
        if (inkStory == null || !inkStory.canContinue)
        {
            EndDialogue();
            return;
        }

        // 讀取下一行文字
        string line = inkStory.Continue().Trim();

        // 解析 "名字: 台詞" 格式
        // 例如 "MAI嚮導: 你好！" -> who="MAI嚮導", text="你好！"
        string who = "";
        string text = line;

        int colonIndex = line.IndexOf(':'); // 尋找全形或半形冒號，這裡假設是用半形 ':'
        if (colonIndex > 0)
        {
            who = line.Substring(0, colonIndex).Trim();
            text = line.Substring(colonIndex + 1).Trim();
        }
        // 如果您 Ink 裡用全形冒號 '：'，可以加一個檢查：
        else if ((colonIndex = line.IndexOf('：')) > 0)
        {
            who = line.Substring(0, colonIndex).Trim();
            text = line.Substring(colonIndex + 1).Trim();
        }

        currentLineText = text;

        // 更新 UI
        if (nameText) nameText.text = who;
        if (bodyText) bodyText.text = ""; // 先清空，準備打字

        // 執行打字或直接顯示
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

    // ============================================================
    //  打字機協程
    // ============================================================
    IEnumerator TypeText(string content)
    {
        float t = 0;
        int charIndex = 0;
        int totalChars = content.Length;

        while (charIndex < totalChars)
        {
            t += Time.deltaTime * Mathf.Max(1, charsPerSecond);
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, totalChars);

            if (bodyText)
                bodyText.text = content.Substring(0, charIndex);

            yield return null;
        }

        // 打字完成
        if (bodyText) bodyText.text = content;
        typingCo = null;
        if (continueHint) continueHint.SetActive(true);
    }

    // ============================================================
    //  結束對話
    // ============================================================
    void EndDialogue()
    {
        Debug.Log("🟥 DialogueController: 對話結束");

        if (panelRoot) panelRoot.SetActive(false);

        inkStory = null; // 清除故事狀態

        // 通知 GameFlow 對話已結束 (可能會觸發戰鬥等)
        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}