using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerMove;              
    public PlayerControllerFight playerFight;        
    public DialogueController dialogue;              
    public GameObject mai;                           
    public GameObject enemiesRoot;                   
    public GameObject maiHelpArea; // ---MAI幫助區---

    private bool inDialogue = false;
    private bool canTalk = false;

    void Start()
    {
        // 一開始關閉戰鬥相關控制與幫助區
        if (playerFight) playerFight.enabled = false;
        if (enemiesRoot) enemiesRoot.SetActive(false);
        if (maiHelpArea) maiHelpArea.SetActive(false);
    }

    void Update()
    {
        // 可以對話時按 E 啟動
        if (canTalk && !inDialogue && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    public void EnableTalk(bool value)
    {
        canTalk = value;
        // 加Press E提示的顯示
    }

    public void StartDialogue()
    {
        inDialogue = true;
        if (playerMove) playerMove.enabled = false;
        if (maiHelpArea) maiHelpArea.SetActive(false); // 關閉幫助區
        if (dialogue) dialogue.StartDialogue("intro");
    }

    // DialogueController 結束時呼叫
    public void OnDialogueFinished()
    {
        inDialogue = false;
        if (mai) mai.SetActive(false);                // MAI 消失
        if (playerFight) playerFight.enabled = true;  // 開啟戰鬥控制
        if (enemiesRoot) enemiesRoot.SetActive(true); // 敵人出現
        if (maiHelpArea) maiHelpArea.SetActive(true); // 幫助區開啟
    }
}
