using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI ���O�]�w")]
    public GameObject[] menuPanels;
    public GameObject defaultPanel; // �o�O�z���U E �|�}�Ҫ����ӭ��O (�I�])

    private bool isMenuOpen = false;
    private bool hasShownTutorial = false; // �O���O�_�w�g�йL

    void Start()
    {
        HideAllPanels();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueController.Instance != null && DialogueController.Instance.IsPlaying) return;

            if (InventoryManager.Instance != null && !InventoryManager.Instance.isUnlocked)
            {
                Debug.Log("�@���٨S��A����}�ҭI�]�I");
                return; // �����פU�ӡA������᭱���}���ʧ@
            }

            // �U���O�쥻���`���}���޿�
            if (isMenuOpen)
            {
                HideAllPanels();
            }
            else
            {
                ShowDefaultPanel();
            }
        }
    }

    public void ShowDefaultPanel()
    {
        HideAllPanels();

        if (defaultPanel != null)
        {
            defaultPanel.SetActive(true);
            isMenuOpen = true;

            // Ĳ�o�о��޿�
            if (!hasShownTutorial)
            {
                if (TutorialManager.Instance != null)
                {
                    Debug.Log("�Ĥ@�����}�I�]�AĲ�o�оǡI");
                    hasShownTutorial = true;
                    TutorialManager.Instance.OpenTutorial();
                }
            }
        }
    }

    public void SwitchToPanel(GameObject panelToShow)
    {
        HideAllPanels();
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            isMenuOpen = true;
        }
    }

    public void HideAllPanels()
    {
        foreach (GameObject panel in menuPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        isMenuOpen = false;
    }
}