using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow;

    [Header("UI")]
    [Tooltip("整個對話面板，例如 Canvas 下的 DialoguePanel")]
    public GameObject panelRoot;          // DialoguePanel
    public TMP_Text nameText;             // NameText
    public TMP_Text bodyText;             // BodyText
    public GameObject continueHint;       // ContinueHint (可以是小圖示)

    [Header("Typing")]
    public bool typewriter = true;
    public float charsPerSecond = 40f;

    Coroutine typingCo;
    string lastSpeaker = "";
    static readonly Regex SPEAKER = new(@"^\s*([^:：]+)\s*[:：]\s*(.*)$");

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

    // ----------------------------------------------------
    // 初始化：一開始關掉對話 UI
    // ----------------------------------------------------
    void Awake()
    {
        if (panelRoot)
            panelRoot.SetActive(false);

        if (continueHint)
            continueHint.SetActive(false);
    }

    // ----------------------------------------------------
    // 給外部呼叫：開始一整段對話
    // ----------------------------------------------------
    public void StartDialogue()
    {
        Debug.Log("🟦 DialogueController.StartDialogue");

        if (!panelRoot)
        {
            Debug.LogError("DialogueController: panelRoot 沒指定！");
            return;
        }

        // 重設狀態
        _mockIndex = 0;
        lastSpeaker = "";
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }

        panelRoot.SetActive(true);            // 開啟整個 DialoguePanel
        if (continueHint) continueHint.SetActive(false);
        if (bodyText) bodyText.text = "";
        if (nameText) nameText.text = "";

        // 通知 GameFlow 進入「Talking」狀態（會鎖玩家移動、關掉 MAI 幫助區）
        if (gameFlow)
            gameFlow.OnDialogueStarted();

        // 確保 UI 已經 active 再開始顯示文字
        StartCoroutine(StartAfterUIReady());
    }

    IEnumerator StartAfterUIReady()
    {
        // 等一 frame，讓 Unity 把 active 狀態更新完
        yield return null;

        // 再保險：直到整個階層都 active 才開始
        while (!panelRoot.activeInHierarchy)
            yield return null;

        Advance();
    }

    // ----------------------------------------------------
    // Update：按滑鼠左鍵 / 空白鍵 → 下一句
    // ----------------------------------------------------
    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf)
            return;

        if (continueHint && continueHint.activeSelf &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            Advance();
        }
    }

    // ----------------------------------------------------
    // 讀取下一句台詞
    // ----------------------------------------------------
    void Advance()
    {
        // 1. 如果正在打字 → 直接跳完本句
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;

            // 補上完整句子
            if (_mockIndex > 0 && _mockIndex <= _mockLines.Length)
            {
                string lastLine = _mockLines[_mockIndex - 1];
                var mLast = SPEAKER.Match(lastLine);
                string fullText = mLast.Success ? mLast.Groups[2].Value : lastLine;
                if (bodyText) bodyText.text = fullText;
            }

            if (continueHint) continueHint.SetActive(true);
            return;
        }

        // 2. 對話已經講完 → 結束
        if (_mockIndex >= _mockLines.Length)
        {
            End();
            return;
        }

        // 3. 讀下一句
        string line = _mockLines[_mockIndex++];

        var m = SPEAKER.Match(line);
        string who = lastSpeaker;
        string text = line;

        if (m.Success)
        {
            who = m.Groups[1].Value.Trim();
            text = m.Groups[2].Value;
            lastSpeaker = who;
        }

        if (nameText) nameText.text = who;

        if (!typewriter)
        {
            if (bodyText) bodyText.text = text;
            if (continueHint) continueHint.SetActive(true);
        }
        else
        {
            if (continueHint) continueHint.SetActive(false);
            typingCo = StartCoroutine(TypeText(text));
        }
    }

    // ----------------------------------------------------
    // 打字機效果
    // ----------------------------------------------------
    IEnumerator TypeText(string t)
    {
        if (bodyText) bodyText.text = "";

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
                if (bodyText)
                    bodyText.text = t[..shown];
            }

            yield return null;
        }

        typingCo = null;
        if (continueHint) continueHint.SetActive(true);
    }

    // ----------------------------------------------------
    // 結束對話
    // ----------------------------------------------------
    void End()
    {
        Debug.Log("🟥 Dialogue 結束");

        if (panelRoot)
            panelRoot.SetActive(false);

        if (continueHint)
            continueHint.SetActive(false);

        // 告訴 GameFlow：對話結束 → 進入戰鬥、開啟敵人
        if (gameFlow)
            gameFlow.OnDialogueFinished();
    }
}
