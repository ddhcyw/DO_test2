using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;
using Core;   // 使用 TrainingManager

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

    // ============================================================
    //  綁定外部函數 (Ink -> C#)
    // ============================================================
    void BindExternal()
    {
        if (inkStory == null)
        {
            Debug.LogError("DialogueController: inkStory 還沒建立就嘗試綁定 external");
            return;
        }

        if (gameFlow == null)
        {
            Debug.LogWarning("DialogueController: GameFlow 未指定，僅會呼叫 TrainingManager 的外部功能");
        }

        // 1. 給予相機 (~ give_camera())
        inkStory.BindExternalFunction("give_camera", () =>
        {
            if (gameFlow != null)
                gameFlow.GiveCamera();

            if (TrainingManager.Instance != null)
                TrainingManager.Instance.OnGiveCamera();
        });

        // 2. 顯示任務指示 (~ show_objective("目標", "提示"))
        inkStory.BindExternalFunction("show_objective", (string target, string hint) =>
        {
            if (gameFlow != null)
                gameFlow.ShowObjectiveUI(target, hint);

            if (TrainingManager.Instance != null)
                TrainingManager.Instance.ShowObjective(target, hint);
        });

        // 3. 產生練習場怪物 (~ spawn_wave())
        inkStory.BindExternalFunction("spawn_wave", () =>
        {
            // 生成練習用數據蟲
            if (TrainingManager.Instance != null)
            {
                TrainingManager.Instance.StartTraining();
            }
            else if (gameFlow != null)
            {
                gameFlow.SetSpawnTrainingBugAfterDialogue();
            }
            else
            {
                Debug.LogWarning("spawn_wave 被呼叫，但場景中沒有 TrainingManager 或 GameFlow。");
            }

            // 這個節點我們就是要「生完怪 → 收對話 → 讓玩家動」
            EndDialogue();
        });
    }


    // ============================================================
    //  讀取下一句對話
    // ============================================================
    void ContinueInk()
    {
        if (inkStory == null || !inkStory.canContinue)
        {
            EndDialogue();
            return;
        }

        string line = inkStory.Continue().Trim();

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

        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}
