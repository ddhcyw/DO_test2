using UnityEngine;
using System.Collections;
using System.Collections.Generic; // �Ω� Queue
using TMPro;
using UnityEngine.UI;

// --- �Ĥ@�����G��ܸ�ƪ��e�� ---
// �o�� ScriptableObject �i�H���A�b Project �������إ߹�� "�겣"
// �k�� -> Create -> Story -> New Dialogue Sequence

[System.Serializable]
public class DialogueLine
{
    [Tooltip("��ܪ̪��W�r")]
    public string speakerName;

    [Tooltip("��ܤ��e"), TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("�i��G��ܪ̪��Y��")]
    public Sprite speakerPortrait;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Story/New Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> dialogueLines;
}


// --- �ĤG�����G�G�ƹ�ܺ޲z�� ---
// �t�d�b UI �W��ܹ�ܤ��e

public class StoryDialogueManager : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("��ӹ�ܵ������e��")]
    public GameObject dialoguePanel;
    [Tooltip("��ܻ��ܪ̦W�r����r��")]
    public TextMeshProUGUI speakerNameText;
    [Tooltip("��ܹ�ܤ��e����r��")]
    public TextMeshProUGUI dialogueContentText;
    [Tooltip("��ܻ��ܪ��Y�����Ϥ�")]
    public Image speakerPortraitImage;
    [Tooltip("���ܪ��a�~�򪺹ϥܡA�Ҧp�@�Ӱ{�{���b�Y")]
    public GameObject continueIndicator;

    [Header("���r���ĪG�]�w")]
    [Tooltip("�C�Ӧr�X�{�����j�ɶ�")]
    public float typingSpeed = 0.04f;

    // �Ω��x�s��e��ܪ��Ҧ��y�l
    private Queue<DialogueLine> sentences;
    // �Ω�P�_��e�y�l�O�_���b���񥴦r���ĪG
    private bool isTyping = false;
    // �Ω��x�s��e���b��ܪ�����y�l
    private string currentFullSentence;

    // �ϥγ�ҼҦ��A��K��L�}���H�ɩI�s
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
    /// �}�l�@�q�s�����
    /// </summary>
    /// <param name="sequence">�n���񪺹�ܸ}���겣</param>
    public void StartDialogue(DialogueSequence sequence)
    {
        // ��ܹ�ܵ���
        dialoguePanel.SetActive(true);
        // �M�Ť��e����ܦ�C
        sentences.Clear();

        // �N�Ҧ���ܥy�l�[�J��C
        foreach (DialogueLine line in sequence.dialogueLines)
        {
            sentences.Enqueue(line);
        }

        // ��ܲĤ@�y���
        DisplayNextSentence();
    }

    /// <summary>
    /// ��ܤU�@�y��ܡA�Φb��ܵ�������������
    /// </summary>
    public void DisplayNextSentence()
    {
        // �p�G���b���r�A�h�ߨ���ܧ���y�l
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueContentText.text = currentFullSentence;
            isTyping = false;
            continueIndicator.SetActive(true);
            return;
        }

        // �p�G�Ҧ��y�l�������F�A�N�������
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // �q��C�����X�U�@�y���
        DialogueLine currentLine = sentences.Dequeue();

        // ��s UI
        speakerNameText.text = currentLine.speakerName;

        // ��s�Y���]�p�G�����ܡ^
        if (currentLine.speakerPortrait != null)
        {
            speakerPortraitImage.sprite = currentLine.speakerPortrait;
            speakerPortraitImage.enabled = true;
        }
        else
        {
            speakerPortraitImage.enabled = false;
        }

        // �x�s����y�l�A�ö}�l���r���ĪG
        currentFullSentence = currentLine.dialogueText;
        StartCoroutine(TypeSentence(currentFullSentence));
    }

    // ���r���ĪG����{
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

    // �������
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // �b Update ����ť�ƹ��I���A�H�~����
    void Update()
    {
        // �u���b��ܵ�����ܮɤ~��ť
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }
}


// --- �ĤT�����G���Ĳ�o�� (�d��) ---
// �i�H���b NPC ��Ĳ�o�ϰ�W

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("�nĲ�o����ܸ}���겣")]
    public DialogueSequence dialogueToTrigger;

    // �z�i�H�ھڻݨD���Ĳ�o�覡
    // �d��1�G���a�i�JĲ�o���d���
    private void OnTriggerEnter(Collider other)
    {
        // ���]���a�����ҬO "Player"
        if (other.CompareTag("Player"))
        {
            // �I�s��ܺ޲z���Ӷ}�l���
            StoryDialogueManager.instance.StartDialogue(dialogueToTrigger);
            // ���F�����Ĳ�o�A�i�H��ܸT�Φ�Ĳ�o��
            // GetComponent<Collider>().enabled = false;
        }
    }

    // �d��2�G���Ѥ@�Ӥ��}�禡�A����L�}���]�p���ʨt�Ρ^�I�s
    public void TriggerDialogue()
    {
        StoryDialogueManager.instance.StartDialogue(dialogueToTrigger);
    }
}
