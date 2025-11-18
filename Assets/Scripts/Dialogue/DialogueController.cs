using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using Ink.Runtime;   // ★★★ 新增的：Ink 支援

public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow;

    [Header("UI")]
    public GameObject panelRoot;         // DialoguePanel
    public TMP_Text nameText;            // NameText
    public TMP_Text bodyText;            // BodyText
    public GameObject continueHint;      // ContinueHint（小箭頭）

    [Header("Typing")]
    public bool typewriter = true;
    public float charsPerSecond = 40f;

    Coroutine typingCo;
    string lastSpeaker = "";
    static readonly Regex SPEAKER = new(@"^\s*([^:：]+)\s*[:：]\s*(.*)$");

    // ============================================================
    // 🔵 Mock 對話（保留給你測試用）
    // ============================================================
    int _mockIndex = 0;
    readonly string[] _mockLines =
    {
        "MAI: 哈囉！我是 AI 嚮導麻伊！",
        "MAI: 歡迎來到網路城的 AI 區，請問有什麼需要幫忙的嗎？",
        "主角: 我不知道為什麼會進來這裡……。",
        "MAI: 咕？等等，麻伊好像沒有在網路城見過你……。",
        "MAI: 正在查詢用戶身份……咦？",
        "MAI: 難道你不是網路城的居民嗎？好酷喔咕！",
        "MAI: 雖然過去網路城也遇過一些穿越來到這裡的旅行者，但麻伊還是第一次實際遇到咕！",
        "主角: 你到底在說什麼……我想回家。",
        "MAI: 你放心好了！麻伊大概知道是怎麼回事，相信一定可以幫助你回到家鄉～！",
        "MAI: 身為網路城裡專業的嚮導，麻伊非常願意指引你回家的路！",
        "主角: 好……！太感謝你了！",
        "MAI: 如果你願意讓麻伊幫忙的話，就到橋的另一邊找我吧！麻伊先去準備一些能幫助你的工具！"
    };

    // ============================================================
    // 🔶 Ink 對話整合
    // ============================================================
    [Header("Ink Dialogue")]
    public bool useInk = false;          // ★ 是否使用 Ink
    public TextAsset inkJSONAsset;       // ★ Ink JSON
    Story inkStory;                      // ★ Ink Story 物件

    // ============================================================
    void Awake()
    {
        if (panelRoot)
            panelRoot.SetActive(false);

        if (continueHint)
            continueHint.SetActive(false);
    }

    // ============================================================
    // 🔷 外部呼叫：開始對話
    // ============================================================
    public void StartDialogue()
    {
        Debug.Log("🟦 DialogueController.StartDialogue");

        if (!panelRoot)
        {
            Debug.LogError("DialogueController: panelRoot 沒指定！");
            return;
        }

        // 開啟 UI
        panelRoot.SetActive(true);
        continueHint.SetActive(false);
        bodyText.text = "";
        nameText.text = "";

        if (gameFlow)
            gameFlow.OnDialogueStarted();

        // ★ 若使用 Ink，先初始化對話
        if (useInk && inkJSONAsset != null)
        {
            inkStory = new Story(inkJSONAsset.text);
            ContinueInk();
            return;
        }

        // ★ 若不用 Ink → 使用 mock 對話
        _mockIndex = 0;
        lastSpeaker = "";

        if (typingCo != null)
            StopCoroutine(typingCo);

        StartCoroutine(StartAfterUIReady());
    }

    IEnumerator StartAfterUIReady()
    {
        yield return null;
        while (!panelRoot.activeInHierarchy)
            yield return null;

        Advance(); // mock 對話
    }

    // ============================================================
    // 🔷 Update：按空白/滑鼠 → 下一句
    // ============================================================
    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf)
            return;

        if (continueHint && continueHint.activeSelf &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            // 若正在打字 → 跳到完整句子
            if (typingCo != null)
            {
                StopCoroutine(typingCo);
                typingCo = null;
                continueHint.SetActive(true);
                return;
            }

            // ★ Ink 下一句
            if (useInk && inkStory != null)
            {
                ContinueInk();
                return;
            }

            // ★ Mock 下一句
            Advance();
        }
    }

    // ============================================================
    // 🔵 Mock 對話：下一句
    // ============================================================
    void Advance()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;

            if (_mockIndex > 0 && _mockIndex <= _mockLines.Length)
            {
                string lastLine = _mockLines[_mockIndex - 1];
                var mLast = SPEAKER.Match(lastLine);
                string fullText = mLast.Success ? mLast.Groups[2].Value : lastLine;
                bodyText.text = fullText;
            }

            continueHint.SetActive(true);
            return;
        }

        if (_mockIndex >= _mockLines.Length)
        {
            End();
            return;
        }

        string line = _mockLines[_mockIndex++];
        var m = SPEAKER.Match(line);

        string who = m.Success ? m.Groups[1].Value.Trim() : lastSpeaker;
        string text = m.Success ? m.Groups[2].Value : line;

        lastSpeaker = who;
        nameText.text = who;

        if (!typewriter)
        {
            bodyText.text = text;
            continueHint.SetActive(true);
        }
        else
        {
            continueHint.SetActive(false);
            typingCo = StartCoroutine(TypeText(text));
        }
    }

    // ============================================================
    // 🔶 Ink：下一句
    // ============================================================
    void ContinueInk()
    {
        if (!inkStory.canContinue)
        {
            End();
            return;
        }

        string line = inkStory.Continue().Trim();

        // 格式：名字: 台詞
        int idx = line.IndexOf(':');
        string who = idx > 0 ? line.Substring(0, idx).Trim() : "";
        string text = idx > 0 ? line.Substring(idx + 1).Trim() : line;

        nameText.text = who;
        bodyText.text = "";

        if (typewriter)
        {
            continueHint.SetActive(false);
            typingCo = StartCoroutine(TypeText(text));
        }
        else
        {
            bodyText.text = text;
            continueHint.SetActive(true);
        }
    }

    // ============================================================
    // 🟡 打字效果
    // ============================================================
    IEnumerator TypeText(string t)
    {
        bodyText.text = "";

        float time = 0f;
        int shown = 0;
        int len = t.Length;

        while (shown < len)
        {
            time += Time.deltaTime * Mathf.Max(1f, charsPerSecond);
            int want = Mathf.Min(len, Mathf.FloorToInt(time));

            if (want != shown)
            {
                shown = want;
                bodyText.text = t[..shown];
            }

            yield return null;
        }

        typingCo = null;
        continueHint.SetActive(true);
    }

    // ============================================================
    // 🔴 對話結束
    // ============================================================
    void End()
    {
        Debug.Log("🟥 Dialogue 結束");

        panelRoot.SetActive(false);
        continueHint.SetActive(false);

        if (gameFlow)
            gameFlow.OnDialogueFinished();
    }
}
