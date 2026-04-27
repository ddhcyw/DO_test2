// NpcInteraction.cs

using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    public string npcName = "Lia";

    // �ޥαz�� DialogueController (���]�z���@�ӳ�ҩΥi�H�z�L FindObject ���)
    private DialogueController dialogueController;

    void Start()
    {
        dialogueController = FindObjectOfType<DialogueController>(); // �۰ʴM��
    }

    public void OnItemDropped(Item item)
    {
        if (item.name == "�ǳ�")
        {
            Debug.Log($"�b {npcName} ���W�ϥΤF {item.name}�I");

            if (dialogueController != null)
            {
                // �ҰʯS�w�� Ink ��ܸ`�I
                // ���� ImagePlaza.ink ���� === plaza_leah_flyer ===
                dialogueController.StartInkDialogue("plaza_leah_flyer");
            }

            // �������~ (�p�G�ݭn)
            InventoryManager.Instance.Remove(item);
        }
        else if (item.name == "���Ȫ��@�~��" && npcName == "Dandadan")
        {
            Debug.Log($"�N�@�~���浹�F {npcName}");

            if (dialogueController != null)
            {
                // �����x�j�Ҫ����
                dialogueController.StartInkDialogue("dandadan_portfolio");
            }
            NpcClickInteract clickInteract = GetComponent<NpcClickInteract>();
            if (clickInteract != null) clickInteract.dialogueCompleted = true;
        }
        else if (item.name == "���Ȫ��@�~��" && npcName == "good_fortune")
        {
            Debug.Log($"�N�@�~���浹�F {npcName}");

            if (dialogueController != null)
            {
                dialogueController.StartInkDialogue("good_fortune_portfolio");
            }
            NpcClickInteract clickInteract = GetComponent<NpcClickInteract>();
            if (clickInteract != null) clickInteract.dialogueCompleted = true;
        }
        else if (item.name == "���Ȫ��@�~��" && npcName == "cheap_buyer")
        {
            Debug.Log($"�N�@�~���浹�F {npcName}");

            if (dialogueController != null)
            {
                dialogueController.StartInkDialogue("cheap_buyer_portfolio");
            }
            NpcClickInteract clickInteract = GetComponent<NpcClickInteract>();
            if (clickInteract != null) clickInteract.dialogueCompleted = true;
        }
        else
        {
            Debug.Log($"{npcName} �� {item.name} �S������C");
            // �i�H����@�ӳq�Ϊ� "���ݭn" ���
        }
    }
}