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
                // 如果正在打字，瞬間顯示全句
                StopCoroutine(typingCo);
                typingCo = null;
                if (bodyText) bodyText.text = currentLineText;
                if (continueHint) continueHint.SetActive(true);
            }
            else
            {
                // 如果已經顯示完畢，讀下一句
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

        // 1. 建立新的 Story
        inkStory = new Story(inkJSONAsset.text);
        inkStory.allowExternalFunctionFallbacks = true;

        // 2. 綁定外部函數 (關鍵步驟)
        BindExternal();

        // 3. 跳轉到指定節點
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
        if (inkStory == null) return;

        if (gameFlow == null)
        {
            Debug.LogError("DialogueController: GameFlow 未指定，無法執行外部指令！");
            return;
        }

        // --- 新手區指令 ---

        // 1. 給予相機 (讓相機出現在場景中)
        inkStory.BindExternalFunction("give_camera", () => gameFlow.GiveCamera());

        // 2. 撿起相機 (進背包 + 開大圖)
        inkStory.BindExternalFunction("get_camera_item", () => gameFlow.GetCameraItem());

        // 3. 顯示任務指示
        inkStory.BindExternalFunction("show_objective", (string content) => gameFlow.ShowObjectiveUI(content));

        // 4. 產生練習場怪物 (對話結束後生怪)
        inkStory.BindExternalFunction("spawn_wave", () => gameFlow.SetSpawnTrainingBugAfterDialogue());

        // --- 通用指令 ---

        // 5. 切換場景
        inkStory.BindExternalFunction("change_scene", (string sceneName) => gameFlow.SetSceneToLoad(sceneName));

        // --- 圖像廣場指令 ---

        // 6. 顯示地上的傳單
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());

        // 7. 獲得傳單 (進背包 + 開大圖)
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());

        // 8. 銷毀地上的傳單
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());

        // 9. 獲得作品集 (進背包 + 開大圖)
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());

        // --- 幻影巷指令 ---

        // 10. 開啟找碴小遊戲
        inkStory.BindExternalFunction("start_compare_minigame", () => gameFlow.StartCompareMinigame());
    }

    // ============================================================
    //  讀取下一句對話
    // ============================================================
    void ContinueInk()
    {
        if (inkStory == null)
        {
            EndDialogue();
            return;
        }

        // 連續執行邏輯行，直到遇到文字或結束
        string line = null;
        while (inkStory.canContinue)
        {
            line = inkStory.Continue();
            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
                break; // 找到文字了，跳出迴圈準備顯示
            }
        }

        // 如果跑完迴圈還是沒文字，且故事也不能繼續了，就結束
        if (string.IsNullOrWhiteSpace(line) && !inkStory.canContinue)
        {
            EndDialogue();
            return;
        }

        // 如果剛好讀到最後一行是空的 (罕見情況)，就直接結束
        if (string.IsNullOrWhiteSpace(line))
        {
            EndDialogue();
            return;
        }

        // 解析 "名字: 台詞"
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

        // 執行顯示
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
            charIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, totalChars);

            if (bodyText)
                bodyText.text = content.Substring(0, charIndex);

            yield return null;
        }

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
        if (continueHint) continueHint.SetActive(false);

        inkStory = null;

        // 通知 GameFlow 對話已結束 (可能會觸發戰鬥、切換場景等)
        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}