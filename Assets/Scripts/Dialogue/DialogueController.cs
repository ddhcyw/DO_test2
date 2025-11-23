using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    [Tooltip("有需要用到外部指令(給相機、切場景… )的場景才要指定")]
    public GameFlow gameFlow; 

    // 提供其他系統判斷「對話面板是不是開著」
    public bool IsPlaying => panelRoot != null && panelRoot.activeSelf;

    [Header("UI Components")]
    public GameObject panelRoot;    
    public TMP_Text nameText;       
    public TMP_Text bodyText;       
    public GameObject continueHint; 

    [Header("Typing Settings")]
    public bool typewriter = true;      
    public float charsPerSecond = 40f;  

    [Header("Ink Data")]
    public TextAsset inkJSONAsset; 

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

    // ============================================================
    //  啟動對話
    // ============================================================
    public void StartInkDialogue(string knotName)
    {
        Debug.Log($"[Dialogue] 啟動對話節點 '{knotName}'");

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
        if (inkStory == null) return;

        // 如果這個場景沒有 GameFlow，就當「純對話 UI」使用
        if (gameFlow == null)
        {
            Debug.Log("[Dialogue] 沒有指定 GameFlow，這個場景不使用外部指令");
            return;
        }

        // —— 新手區 / 通用指令 —— 

        // 給相機 (~ give_camera())
        inkStory.BindExternalFunction("give_camera", () =>
        {
            gameFlow.GiveCamera();
        });

        // 撿起相機 (~ get_camera_item())
        inkStory.BindExternalFunction("get_camera_item", () =>
        {
            gameFlow.GetCameraItem();
        });

        // 顯示任務目標（新版只有一段文字）(~ show_objective("內容") )
        inkStory.BindExternalFunction("show_objective", (string content) =>
        {
            gameFlow.ShowObjectiveUI(content);
        });

        // 練習場：對話結束後生怪 (~ spawn_wave())
        inkStory.BindExternalFunction("spawn_wave", () =>
        {
            gameFlow.SetSpawnTrainingBugAfterDialogue();
        });

        // 切換場景 (~ change_scene("SceneName"))
        inkStory.BindExternalFunction("change_scene", (string sceneName) =>
        {
            gameFlow.SetSceneToLoad(sceneName);
        });

        // 圖像廣場：顯示 / 撿起 / 銷毀傳單
        inkStory.BindExternalFunction("show_flyer", () => gameFlow.ShowFlyerInScene());
        inkStory.BindExternalFunction("get_flyer", () => gameFlow.GetFlyerItem());
        inkStory.BindExternalFunction("destroy_flyer", () => gameFlow.DestroyFlyerObject());

        // 圖像廣場：獲得作品集
        inkStory.BindExternalFunction("get_portfolio", () => gameFlow.GetPortfolioItem());

        // 幻影巷：開啟找碴小遊戲
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

        string line = null;

        // 先把可能的空行 / 純邏輯跑掉
        while (inkStory.canContinue)
        {
            line = inkStory.Continue();
            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(line) && !inkStory.canContinue)
        {
            EndDialogue();
            return;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            EndDialogue();
            return;
        }

        // 解析「名字: 台詞」
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
    //  打字機
    // ============================================================
    IEnumerator TypeText(string content)
    {
        float t = 0;
        int totalChars = content.Length;

        while (t < totalChars)
        {
            t += Time.deltaTime * Mathf.Max(1, charsPerSecond);
            int charIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, totalChars);

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
        Debug.Log("[Dialogue] 對話結束");

        if (panelRoot) panelRoot.SetActive(false);
        if (continueHint) continueHint.SetActive(false);

        inkStory = null;

        if (gameFlow) gameFlow.OnDialogueFinished();
    }
}
