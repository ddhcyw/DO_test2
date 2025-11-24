using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;
using UnityEngine.UI;   // ← 一定要有，Button 用得到

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

    [Header("Choice UI")]
    public RectTransform choicesRoot;      // 放選項按鈕的容器
    public GameObject choiceButtonPrefab;  // 選項按鈕 Prefab（白色框 + TMP 文字）

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
        ClearChoices();
    }

    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf) return;

        // 如果現在有 Ink 選項，就讓 UI Button 處理，不要吃滑鼠點擊繼續對話
        if (inkStory != null && inkStory.currentChoices.Count > 0)
            return;

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

        // 建立新的 Story
        inkStory = new Story(inkJSONAsset.text);
        inkStory.allowExternalFunctionFallbacks = true;

        // 綁定外部函數
        BindExternal();

        // 跳轉到指定節點
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

        // 開啟 UI
        panelRoot.SetActive(true);
        if (continueHint) continueHint.SetActive(false);
        if (nameText) nameText.text = "";
        if (bodyText) bodyText.text = "";
        ClearChoices();

        if (gameFlow) gameFlow.OnDialogueStarted();

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
            Debug.LogWarning("DialogueController: GameFlow 未指定，外部指令會失效。");
            return;
        }

        // --- 新手區指令 ---
        inkStory.BindExternalFunction("give_camera", () => gameFlow.GiveCamera());
        inkStory.BindExternalFunction("get_camera_item", () => gameFlow.GetCameraItem());
        inkStory.BindExternalFunction("show_objective", (string content) => gameFlow.ShowObjectiveUI(content));
        inkStory.BindExternalFunction("spawn_wave", () => gameFlow.SetSpawnTrainingBugAfterDialogue());

        // --- 通用指令 ---
        inkStory.BindExternalFunction("change_scene", (string sceneName) => gameFlow.SetSceneToLoad(sceneName));

        // --- 圖像廣場 ---
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());

        // --- 幻影巷 ---
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

        // 進下一句前，先把舊選項清掉
        ClearChoices();

        string line = null;

        // 連續執行邏輯行，直到遇到文字或故事結束
        while (inkStory.canContinue)
        {
            line = inkStory.Continue();
            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
                break;
            }
        }

        // 沒文字且不能再繼續：可能已到結尾
        if (string.IsNullOrWhiteSpace(line) && !inkStory.canContinue)
        {
            // 如果這時候有 choices，就開選項
            if (inkStory.currentChoices.Count > 0)
            {
                RefreshChoicesUI();
                return;
            }

            EndDialogue();
            return;
        }

        // 保險：如果還是空行就結束
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

        // 顯示文字
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

        // 這句話後面如果跟著 Ink 選項，就生出按鈕
        if (inkStory.currentChoices.Count > 0)
        {
            RefreshChoicesUI();
        }
    }

    // ============================================================
    //  選項 UI
    // ============================================================
    void RefreshChoicesUI()
    {
        if (choicesRoot == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("DialogueController: 沒有設定 choicesRoot 或 choiceButtonPrefab，無法顯示選項。");
            return;
        }

        ClearChoices();

        var currentChoices = inkStory.currentChoices;
        for (int i = 0; i < currentChoices.Count; i++)
        {
            var choice = currentChoices[i];

            // 生成按鈕
            GameObject btnGO = Instantiate(choiceButtonPrefab, choicesRoot, false);

            // 設文字
            var label = btnGO.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choice.text;

            // 綁定按鈕事件
            int choiceIndex = i;
            var uiButton = btnGO.GetComponent<Button>();
            if (uiButton != null)
            {
                uiButton.onClick.AddListener(() => OnClickChoice(choiceIndex));
            }
        }
    }

    void ClearChoices()
    {
        if (choicesRoot == null) return;

        for (int i = choicesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(choicesRoot.GetChild(i).gameObject);
        }
    }

    void OnClickChoice(int index)
    {
        if (inkStory == null) return;

        Debug.Log($"[Dialogue] Choice clicked index = {index}");

        inkStory.ChooseChoiceIndex(index);

        // 清掉舊的選項
        ClearChoices();

        // 往下繼續劇本
        ContinueInk();
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

        ClearChoices();
        inkStory = null;

        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}
