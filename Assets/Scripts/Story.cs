using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 用於 Queue
using TMPro;
using UnityEngine.UI;

// --- 第一部分：對話資料的容器 ---
// 這個 ScriptableObject 可以讓你在 Project 視窗中建立對話 "資產"
// 右鍵 -> Create -> Story -> New Dialogue Sequence

[System.Serializable]
public class DialogueLine
{
    [Tooltip("對話者的名字")]
    public string speakerName;

    [Tooltip("對話內容"), TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("可選：對話者的頭像")]
    public Sprite speakerPortrait;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Story/New Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> dialogueLines;
}


// --- 第二部分：故事對話管理器 ---
// 負責在 UI 上顯示對話內容

public class StoryDialogueManager : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("整個對話視窗的容器")]
    public GameObject dialoguePanel;
    [Tooltip("顯示說話者名字的文字框")]
    public TextMeshProUGUI speakerNameText;
    [Tooltip("顯示對話內容的文字框")]
    public TextMeshProUGUI dialogueContentText;
    [Tooltip("顯示說話者頭像的圖片")]
    public Image speakerPortraitImage;
    [Tooltip("提示玩家繼續的圖示，例如一個閃爍的箭頭")]
    public GameObject continueIndicator;

    [Header("打字機效果設定")]
    [Tooltip("每個字出現的間隔時間")]
    public float typingSpeed = 0.04f;

    // 用於儲存當前對話的所有句子
    private Queue<DialogueLine> sentences;
    // 用於判斷當前句子是否正在播放打字機效果
    private bool isTyping = false;
    // 用於儲存當前正在顯示的完整句子
    private string currentFullSentence;

    // 使用單例模式，方便其他腳本隨時呼叫
    public static StoryDialogueManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            sentences = new Queue<DialogueLine>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 開始一段新的對話
    /// </summary>
    /// <param name="sequence">要播放的對話腳本資產</param>
    public void StartDialogue(DialogueSequence sequence)
    {
        // 顯示對話視窗
        dialoguePanel.SetActive(true);
        // 清空之前的對話佇列
        sentences.Clear();

        // 將所有對話句子加入佇列
        foreach (DialogueLine line in sequence.dialogueLines)
        {
            sentences.Enqueue(line);
        }

        // 顯示第一句對話
        DisplayNextSentence();
    }

    /// <summary>
    /// 顯示下一句對話，或在對話結束時關閉視窗
    /// </summary>
    public void DisplayNextSentence()
    {
        // 如果正在打字，則立刻顯示完整句子
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueContentText.text = currentFullSentence;
            isTyping = false;
            continueIndicator.SetActive(true);
            return;
        }

        // 如果所有句子都說完了，就結束對話
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // 從佇列中取出下一句對話
        DialogueLine currentLine = sentences.Dequeue();

        // 更新 UI
        speakerNameText.text = currentLine.speakerName;

        // 更新頭像（如果有的話）
        if (currentLine.speakerPortrait != null)
        {
            speakerPortraitImage.sprite = currentLine.speakerPortrait;
            speakerPortraitImage.enabled = true;
        }
        else
        {
            speakerPortraitImage.enabled = false;
        }

        // 儲存完整句子，並開始打字機效果
        currentFullSentence = currentLine.dialogueText;
        StartCoroutine(TypeSentence(currentFullSentence));
    }

    // 打字機效果的協程
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        continueIndicator.SetActive(false);
        dialogueContentText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueContentText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        continueIndicator.SetActive(true);
    }

    // 結束對話
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // 在 Update 中監聽滑鼠點擊，以繼續對話
    void Update()
    {
        // 只有在對話視窗顯示時才監聽
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }
}


// --- 第三部分：對話觸發器 (範例) ---
// 可以掛在 NPC 或觸發區域上

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("要觸發的對話腳本資產")]
    public DialogueSequence dialogueToTrigger;

    // 您可以根據需求選擇觸發方式
    // 範例1：當玩家進入觸發器範圍時
    private void OnTriggerEnter(Collider other)
    {
        // 假設玩家的標籤是 "Player"
        if (other.CompareTag("Player"))
        {
            // 呼叫對話管理器來開始對話
            StoryDialogueManager.instance.StartDialogue(dialogueToTrigger);
            // 為了防止重複觸發，可以選擇禁用此觸發器
            // GetComponent<Collider>().enabled = false;
        }
    }

    // 範例2：提供一個公開函式，讓其他腳本（如互動系統）呼叫
    public void TriggerDialogue()
    {
        StoryDialogueManager.instance.StartDialogue(dialogueToTrigger);
    }
}
