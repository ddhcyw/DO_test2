using UnityEngine;
using UnityEngine.UI;
using System.Collections; // �����ޥγo�Ӥ~��� Coroutine

public class MiniGame_SpotDifference : MonoBehaviour
{
    [Header("�C���]�w")]
    public Button[] targetButtons;
    public GameObject[] scoreIcons;

    [Header("��ӫ�]�w")]
    public bool closeOnWin = true;
    public float winDelay = 1.5f; // �s�W�G��ӫ᩵��X���~����
    public string winInkKnot = "minigame_success";

    private int currentScore = 0;
    private bool isGameFinished = false; // ����b�����������Ĳ�o

    void OnEnable()
    {
        ResetGame();
    }

    void ResetGame()
    {
        currentScore = 0;
        isGameFinished = false;

        foreach (var icon in scoreIcons)
        {
            if (icon != null) icon.SetActive(false);
        }

        foreach (var btn in targetButtons)
        {
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                Button tempBtn = btn;
                btn.onClick.AddListener(() => OnCorrectButtonClicked(tempBtn));
            }
        }
    }

    void OnCorrectButtonClicked(Button clickedBtn)
    {
        // �p�G�C���w�g����(���b�]����)�A�N���B�z�I��
        if (isGameFinished) return;

        clickedBtn.interactable = false;

        // ��ܿn���ϥ�
        if (currentScore < scoreIcons.Length)
        {
            if (scoreIcons[currentScore] != null)
            {
                scoreIcons[currentScore].SetActive(true);
            }
        }

        currentScore++;

        // �ˬd�O�_�L��
        if (currentScore >= targetButtons.Length)
        {
            // �Ұ���Ӭy�{
            StartCoroutine(WinSequence());
        }
    }

    IEnumerator WinSequence()
    {
        isGameFinished = true;
        Debug.Log("小遊戲勝利！等待後繼續對話...");

        yield return new WaitForSeconds(winDelay);

        if (closeOnWin)
        {
            gameObject.SetActive(false);
        }

        // 通知對話系統繼續播放小遊戲後的對話
        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.TempShowAndContinue();
        }
        else
        {
            Debug.LogWarning("MiniGame_SpotDifference: 找不到 DialogueController.Instance，對話無法繼續！");
        }
    }
}