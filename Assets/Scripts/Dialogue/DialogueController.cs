using UnityEngine;
using TMPro;
using System.Collections;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    [Header("GameFlow Reference")]
    public GameFlow gameFlow;

    [Header("UI")]
    public GameObject panelRoot;   // DialoguePanel
    public TMP_Text nameText;      // NameText
    public TMP_Text bodyText;      // BodyText
    public GameObject continueHint; // 小箭頭

    [Header("Typing")]
    public bool typewriter = true;
    public float charsPerSecond = 40f;

    [Header("Ink Dialogue")]
    public TextAsset inkJSONAsset; // Ink 編譯出的 JSON

    Story inkStory;
    Coroutine typingCo;
    string currentKnotName = "";
    string currentLineText = "";

    void Awake()
    {
        if (panelRoot)
            panelRoot.SetActive(false);

        if (continueHint)
            continueHint.SetActive(false);
    }

    // ============================================================
    // 外部呼叫：從指定 Ink knot 開始對話
    // 例：dialogue.StartInkDialogue("bridge_intro");
    // ============================================================
    public void StartInkDialogue(string knotName)
    {
        Debug.Log($"🟦 DialogueController.StartInkDialogue({knotName})");

        if (!panelRoot)
        {
            Debug.LogError("DialogueController: panelRoot 沒指定！");
            return;
        }

        if (inkJSONAsset == null)
        {
            Debug.LogError("DialogueController: inkJSONAsset 沒指定！");
            return;
        }

        currentKnotName = knotName;

        // 每次開對話都重建一個新的 Story，避免舊狀態殘留
        inkStory = new Story(inkJSONAsset.text);

        if (!string.IsNullOrEmpty(currentKnotName))
        {
            try
            {
                inkStory.ChoosePathString(currentKnotName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DialogueController: 找不到 Ink knot {currentKnotName}: {e}");
            }
        }

        // 開啟 UI
        panelRoot.SetActive(true);
        if (continueHint) continueHint.SetActive(false);
        if (bodyText) bodyText.text = "";
        if (nameText) nameText.text = "";

        if (gameFlow)
            gameFlow.OnDialogueStarted();

        // 顯示第一句
        ContinueInk();
    }

    // ============================================================
    // Update：按空白/滑鼠 → 跳過打字 or 下一句
    // ============================================================
    void Update()
    {
        if (!panelRoot || !panelRoot.activeSelf)
            return;

        if (continueHint && continueHint.activeSelf &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            // 還在打字 → 跳到完整句子
            if (typingCo != null)
            {
                StopCoroutine(typingCo);
                typingCo = null;

                if (bodyText)
                    bodyText.text = currentLineText;

                if (continueHint)
                    continueHint.SetActive(true);

                return;
            }

            // 打完了 → 下一句
            ContinueInk();
        }
    }

    // ============================================================
    // Ink：下一句
    // ============================================================
    void ContinueInk()
    {
        if (inkStory == null)
        {
            End();
            return;
        }

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

        currentLineText = text;

        if (nameText)
            nameText.text = who;

        if (bodyText)
            bodyText.text = "";

        if (typewriter)
        {
            if (continueHint)
                continueHint.SetActive(false);

            if (typingCo != null)
                StopCoroutine(typingCo);

            typingCo = StartCoroutine(TypeText(text));
        }
        else
        {
            if (bodyText)
                bodyText.text = text;

            if (continueHint)
                continueHint.SetActive(true);
        }
    }

    // ============================================================
    // 打字效果
    // ============================================================
    IEnumerator TypeText(string t)
    {
        if (bodyText)
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
                if (bodyText)
                    bodyText.text = t.Substring(0, want);
            }

            yield return null;
        }

        typingCo = null;

        if (continueHint)
            continueHint.SetActive(true);
    }

    // ============================================================
    // 對話結束
    // ============================================================
    void End()
    {
        Debug.Log("🟥 Dialogue 結束");

        if (panelRoot)
            panelRoot.SetActive(false);

        if (continueHint)
            continueHint.SetActive(false);

        inkStory = null;
        currentKnotName = "";
        currentLineText = "";

        if (gameFlow)
            gameFlow.OnDialogueFinished();
    }
}
