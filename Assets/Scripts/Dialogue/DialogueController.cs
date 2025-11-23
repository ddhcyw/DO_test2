using UnityEngine;
using UnityEngine.UI;
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

    [Header("Choice UI")]
    public RectTransform choicesRoot;       // 選項容器 (例如 DialoguePanel 底下的 ChoicesRoot)
    public GameObject choiceButtonPrefab;   // 剛做好的 ChoiceButton prefab

    [Header("Typing Settings")]
    public bool typewriter = true;      // 是否開啟打字機效果
    public float charsPerSecond = 40f;  // 打字速度

    [Header("Ink Data")]
    public TextAsset inkJSONAsset; // Ink 編譯後的 .json 檔案

    // 內部變數
    Story inkStory;
    Coroutine typingCo;
    string currentLineText = "";
    bool isShowingChoices = false;

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);
        if (choicesRoot) choicesRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf) return;

        // 有選項時，不要讓滑鼠點擊去觸發「下一句」
        if (isShowingChoices) return;

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

        // 2. 綁定外部函數
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

        ClearChoices();
        isShowingChoices = false;

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
        inkStory.BindExternalFunction("give_camera", () => gameFlow.GiveCamera());
        inkStory.BindExternalFunction("get_camera_item", () => gameFlow.GetCameraItem());
        inkStory.BindExternalFunction("show_objective", (string content) => gameFlow.ShowObjectiveUI(content));
        inkStory.BindExternalFunction("spawn_wave", () => gameFlow.SetSpawnTrainingBugAfterDialogue());

        // --- 通用指令 ---
        inkStory.BindExternalFunction("change_scene", (string sceneName) => gameFlow.SetSceneToLoad(sceneName));

        // --- 圖像廣場指令 ---
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());

        // --- 幻影巷指令 ---
        inkStory.BindExternalFunction("start_compare_minigame", () => gameFlow.StartCompareMinigame());
    }

    // ============================================================
    //  讀取下一句對話（包含處理選項）
    // ============================================================
    void ContinueInk()
    {
        if (inkStory == null)
        {
            EndDialogue();
            return;
        }

        // 如果這一刻就已經有選項了，先顯示選項
        if (inkStory.currentChoices.Count > 0)
        {
            ShowChoices();
            return;
        }

        string line = null;

        // 連續執行，略過只呼叫 external、沒有文字的行
        while (inkStory.canContinue)
        {
            line = inkStory.Continue();

            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
                break;
            }

            // 這次 Continue 沒拿到字，但產生了選項
            if (inkStory.currentChoices.Count > 0)
            {
                line = null;
                break;
            }
        }

        // 沒文字可以顯示時，看是結束還是進入選項
        if (string.IsNullOrWhiteSpace(line))
        {
            if (inkStory.currentChoices.Count > 0)
            {
                ShowChoices();
                return;
            }

            if (!inkStory.canContinue)
            {
                EndDialogue();
                return;
            }

            // 還可以 continue，但這次沒字，多半是 external，保險再叫一次
            ContinueInk();
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

        // 打字機或直接顯示
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

        if (!isShowingChoices && continueHint) continueHint.SetActive(true);
    }

    // ============================================================
    //  選項 UI
    // ============================================================
    void ShowChoices()
    {
        if (choicesRoot == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("DialogueController: 沒有設定 choicesRoot 或 choiceButtonPrefab，無法顯示選項。");
            return;
        }

        ClearChoices();

        var choices = inkStory.currentChoices;
        if (choices.Count == 0) return;

        isShowingChoices = true;
        choicesRoot.gameObject.SetActive(true);
        if (continueHint) continueHint.SetActive(false);

        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];

            GameObject btnGO = Instantiate(choiceButtonPrefab, choicesRoot);
            btnGO.name = $"ChoiceButton_{i}";

            Button btn = btnGO.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("DialogueController: ChoiceButton prefab 上沒有 Button 組件。");
                continue;
            }

            TMP_Text txt = btnGO.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = choice.text;

            int index = i; // local copy
            btn.onClick.AddListener(() => OnClickChoice(index));
        }
    }

    void ClearChoices()
    {
        if (choicesRoot == null) return;

        foreach (Transform child in choicesRoot)
        {
            Destroy(child.gameObject);
        }

        choicesRoot.gameObject.SetActive(false);
        isShowingChoices = false;
    }

    void OnClickChoice(int index)
    {
        if (inkStory == null) return;

        Debug.Log($"DialogueController: 選擇了選項 {index}");
        inkStory.ChooseChoiceIndex(index);

        ClearChoices();

        // 選完後繼續劇情
        ContinueInk();
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

        // 通知 GameFlow 對話已結束 (可能會觸發戰鬥、切換場景等)
        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}
