using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Dialogue;
using System;


public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow; // 用來呼叫外部指令
    public bool IsPlaying => panelRoot != null && panelRoot.activeSelf;

    [Header("UI Components")]
    public GameObject panelRoot;    // 對話面板 (DialoguePanel)
    public TMP_Text nameText;       // 顯示名字
    public TMP_Text bodyText;       // 顯示內容
    public GameObject continueHint; // 繼續對話提示

    [Header("Choice UI")]
    public RectTransform choicesRoot;      // 放選項按鈕的容器
    public GameObject choiceButtonPrefab;  // 選項按鈕 Prefab

    [Header("Typing Settings")]
    public bool typewriter = true;
    public float charsPerSecond = 40f;

    [Header("Ink Data")]
    public TextAsset inkJSONAsset;

    [Header("Scene Controllers")]
    public RocketController rocketController;
    public DialogueSequenceRunner sequenceRunner;

    //對話動畫
    [Header("Portrait")]
    public DialoguePortraitSpine portrait;
    public DialogueSpeakerDB speakerDB;

    // 內部狀態
    private Story inkStory;
    private Coroutine typingCo;
    private string currentLineText = "";
    public bool pauseRequested = false;
    private string pendingLine = "";

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);
        ClearChoices();
    }

    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf) return;
        // 如果遊戲暫停了 (例如打開了設定選單)，完全不處理任何輸入
        if (Time.timeScale == 0) return;
        

        // 有選項時，交給 Button 處理滑鼠事件
        if (inkStory != null && inkStory.currentChoices.Count > 0)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
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
        if (tempHidden) return;
    }

    // ============================================================
    // 啟動對話
    // ============================================================
    public void StartInkDialogue(string knotName)
    {
        Debug.Log($"🟦 DialogueController: 啟動對話節點 '{knotName}'");
        if (portrait != null)
        {
            portrait.speakerDB = speakerDB; // 保險
            portrait.gameObject.SetActive(true);
        }


        if (!panelRoot || !inkJSONAsset)
        {
            Debug.LogError(
    $"DialogueController MISSING refs | go='{gameObject.name}' id={GetInstanceID()} " +
    $"panelRoot={(panelRoot?panelRoot.name:"NULL")} ink={(inkJSONAsset?inkJSONAsset.name:"NULL")} " +
    $"scene='{gameObject.scene.name}'"
);

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
        ClearChoices();

        if (gameFlow) gameFlow.OnDialogueStarted();

        ContinueInk();
    }

    // ============================================================
    // Ink 外部函數
    // ============================================================
    void BindExternal()
    {

        if (inkStory == null) return;

        if (gameFlow == null)
        {
            Debug.LogWarning("DialogueController: GameFlow 未指定，外部指令會失效。");
            return;
        }

        // 新手區
        inkStory.BindExternalFunction("give_camera", () => gameFlow.GiveCamera());
        inkStory.BindExternalFunction("get_camera_item", () => gameFlow.GetCameraItem());
        inkStory.BindExternalFunction("show_objective", (string content) => gameFlow.ShowObjectiveUI(content));
        inkStory.BindExternalFunction("spawn_wave", () => gameFlow.SetSpawnTrainingBugAfterDialogue());
        inkStory.BindExternalFunction("hide_mai", (string id) => gameFlow.HideMai(id));
        inkStory.BindExternalFunction("play_ignite_anim", () => rocketController.PlayIgnite());
        inkStory.BindExternalFunction("pause_dialogue", (float seconds) => {
            if (sequenceRunner != null) sequenceRunner.PauseDialogue(seconds);
            else Debug.LogError("DialogueController: sequenceRunner 沒有指定！");
        });

        // 通用
        inkStory.BindExternalFunction("change_scene", (string sceneName) => gameFlow.SetSceneToLoad(sceneName));
        inkStory.BindExternalFunction("add_clue", (string id) => {
            gameFlow.AddClue(id);
        });


        // 圖像廣場
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());

        // 幻影巷
        inkStory.BindExternalFunction("start_MAI_help", () => gameFlow.StartMAIHelp());
        inkStory.BindExternalFunction("start_compare_minigame", (string id) => {
            gameFlow.StartCompareMinigame(id);
        });

        // --- 作品集偷偷 ---
        inkStory.BindExternalFunction("start_debate_round", (string id) => {
            gameFlow.StartDebateRound(id);
        });
        inkStory.BindExternalFunction("open_book", (string nextKnot) => {
            // 1. 先關閉目前的對話框 (因為焦點要轉移到書本上了)
            EndDialogue();
            // 2. 呼叫 GameFlow 開書
            gameFlow.OpenStoryBook(nextKnot);
        });
    }

    // ============================================================
    // 不要顯示動畫名稱
    // ============================================================
    string GetDisplayName(string speakerId)
    {
        if (string.IsNullOrWhiteSpace(speakerId))
            return "";

        if (speakerId.StartsWith("黑色利亞", StringComparison.OrdinalIgnoreCase))
            return "黑色利亞";

        return speakerId;
    }
    // ============================================================
    // 讀取下一句
    // ============================================================
    void ContinueInk()
    {
        if (inkStory == null)
        {
            EndDialogue();
            return;
        }

        // 進下一句前清掉舊選項
        ClearChoices();

        string line = null;
        
        while (inkStory.canContinue)
        {
            line = inkStory.Continue();
            if (pauseRequested)
            {
                pauseRequested = false;
                if (!string.IsNullOrWhiteSpace(line))
                    pendingLine = line.Trim();
                return;
            }
            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
                break;
            }
        }

        // 沒文字且不能繼續：看有沒有選項
        if (string.IsNullOrWhiteSpace(line) && !inkStory.canContinue)
        {
            if (inkStory != null && inkStory.currentChoices.Count > 0)
            {
                RefreshChoicesUI();
                return;
            }

            EndDialogue();
            return;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            EndDialogue();
            return;
        }

        Debug.Log($"[Dialogue] rawLine='{line}'");
        DisplayLine(line);

        // 這句之後如果有選項，顯示選項
        if (inkStory != null && inkStory.currentChoices.Count > 0)
        {
            RefreshChoicesUI();
        }
    }
    // ============================================================
    // 暫時隱藏對話框（不結束對話）     
    bool tempHidden = false;

    public void TempHide()
    {
        tempHidden = true;

        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
        ClearChoices();
        if (continueHint) continueHint.SetActive(false);
        if (panelRoot) panelRoot.SetActive(false);
    }

    public void TempShowAndContinue()
    {
        if (panelRoot) panelRoot.SetActive(true);
        tempHidden = false;
        if (!string.IsNullOrEmpty(pendingLine))
        {
            string line = pendingLine;
            pendingLine = "";
            DisplayLine(line);
        }
        else
        {
            ContinueInk();
        }
    }
    
    // ============================================================
    // 顯示單行對話（解析說話者 + 打字機）
    // ============================================================
    void DisplayLine(string line)
    {
        ClearChoices();

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

        if (nameText) nameText.text = GetDisplayName(who);
        if (portrait != null) portrait.SetSpeaker(who);

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

    // ============================================================
    // 選項 UI
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

        Debug.Log($"[Dialogue] 生成 {currentChoices.Count} 個選項");

        for (int i = 0; i < currentChoices.Count; i++)
        {
            var choice = currentChoices[i];

            GameObject btnGO = Instantiate(choiceButtonPrefab, choicesRoot, false);

            var label = btnGO.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choice.text;

            int choiceIndex = i;
            var uiButton = btnGO.GetComponent<Button>();
            if (uiButton != null)
            {
                uiButton.onClick.AddListener(() => OnClickChoice(choiceIndex));
            }
            else
            {
                Debug.LogError("ChoiceButton prefab 上沒有 Button 組件！");
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
        ClearChoices();
        ContinueInk();
    }

    // ============================================================
    // 打字機
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
        
         // 在文字完整顯示後，才顯示選項
        if (inkStory != null && inkStory.currentChoices.Count > 0)
        {
            RefreshChoicesUI();
        }
    }

    // ============================================================
    // 結束對話
    // ============================================================
    void EndDialogue()
    {
        Debug.Log("🟥 DialogueController: 對話結束");

        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);

        ClearChoices();
        inkStory = null;

        if (gameFlow) gameFlow.OnDialogueFinished();
        if (portrait != null) portrait.Hide();

    }
}