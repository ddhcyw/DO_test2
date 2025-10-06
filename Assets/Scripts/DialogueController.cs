using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections;
#if INK_PRESENT
using Ink.Runtime;
#endif

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public GameObject continueHint;

    [Header("Ink (選填)")]
    public TextAsset inkJSON;          // Intro.ink.json
    public string startKnot = "intro";

    [Header("Typing")]
    public bool typewriter = true;
    public float charsPerSecond = 40f;

#if INK_PRESENT
    Ink.Runtime.Story story;
#endif
    Coroutine typingCo;
    string lastSpeaker = "";
    static readonly Regex SPEAKER = new(@"^\s*([^:：]+)\s*[:：]\s*(.*)$");

    void Awake(){ if(root) root.SetActive(false); }

    public void StartDialogue(string knot = null){
        if(root) root.SetActive(true);
#if INK_PRESENT
        if (inkJSON != null) {
            story = new Ink.Runtime.Story(inkJSON.text);
            BindExternal();
            if (!string.IsNullOrEmpty(knot)) story.ChoosePathString(knot);
        }
#endif
        SendMessageUpwards("OnDialogueStarted", SendMessageOptions.DontRequireReceiver);
        Advance();
    }

    void Update(){
        if (root && !root.activeSelf) return;
        if (continueHint && continueHint.activeSelf &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
            Advance();
    }

    void Advance(){
        if (typingCo != null){ StopCoroutine(typingCo); typingCo = null; if(continueHint) continueHint.SetActive(true); return; }

        string line = null;
#if INK_PRESENT
        if (inkJSON != null){
            if (!(story?.canContinue ?? false)){ End(); return; }
            line = story.Continue().Trim();
            foreach (var tag in story.currentTags) HandleTag(tag);
        } else
#endif
        {
            // —— 無 Ink 後備：放一段假台詞測流程 ——
            if (_mockIdx >= _mockLines.Length){ End(); return; }
            line = _mockLines[_mockIdx++];
        }

        var m = SPEAKER.Match(line ?? "");
        string who = lastSpeaker, text = line;
        if (m.Success){ who = m.Groups[1].Value.Trim(); text = m.Groups[2].Value; lastSpeaker = who; }
        if (nameText) nameText.text = who;
        if (!typewriter){ if(bodyText) bodyText.text = text; if(continueHint) continueHint.SetActive(true); }
        else{ if(continueHint) continueHint.SetActive(false); typingCo = StartCoroutine(TypeText(text)); }
    }

    IEnumerator TypeText(string t){
        if(bodyText) bodyText.text = "";
        float s=0; int shown=0;
        while (shown < t.Length){
            s += Time.deltaTime * Mathf.Max(1, charsPerSecond);
            int want = Mathf.Min(t.Length, Mathf.FloorToInt(s));
            if (want!=shown){ shown=want; bodyText.text = t[..shown]; }
            yield return null;
        }
        typingCo=null; if(continueHint) continueHint.SetActive(true);
    }

    void End(){
        if(root) root.SetActive(false);
        SendMessageUpwards("OnDialogueFinished", SendMessageOptions.DontRequireReceiver);
    }

#if INK_PRESENT
    void BindExternal(){
        story.BindExternalFunction("give_camera", () => SendMessageUpwards("GiveCamera", SendMessageOptions.DontRequireReceiver));
        story.BindExternalFunction("show_hint", (string id) => SendMessageUpwards("ShowHint", id, SendMessageOptions.DontRequireReceiver));
        story.BindExternalFunction("spawn_wave", () => SendMessageUpwards("SpawnDataBugs", SendMessageOptions.DontRequireReceiver));
    }
    void HandleTag(string tag){
        if (tag.StartsWith("speaker=") || tag.StartsWith("name=")){
            var sp = tag.Split('='); lastSpeaker = sp.Length>1 ? sp[1] : lastSpeaker;
            if (nameText) nameText.text = lastSpeaker;
        }
    }
#endif

    // 假資料：Ink 未就緒也能跑流程
    int _mockIdx = 0;
    readonly string[] _mockLines = new[]{
        "MAI嚮導: 你好，用戶！我是 AI 嚮導 MAI。",
        "主角: 我不知道為什麼會進來這裡……。",
        "MAI嚮導: 我會幫你回家，但先完成新手區的小考驗！"
    };
}
