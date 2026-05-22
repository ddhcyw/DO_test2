using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI �]�w")]
    public GameObject tutorialRootPanel; // ��ӱоǪ��̤W�h������
    public List<GameObject> tutorialSteps; // Step1, Step2, Step3...
    [Tooltip("�I���۾����ȲĴX�B(�n-1)")]
    public int clickStepIndex = 5;
    [Header("�즲���ȲĴX�B(�n-1)")]
    public int dragStepIndex = 7;
    [Header("��ӥ��ȲĴX�B(�n-1)")]
    public int takePhotoStepIndex = 13;

    [Header("���A")]
    // ���}�o���ܼơA����L�}���i�HŪ��
    public int CurrentStepIndex = 0;

    // �P�_�оǬO�_���b�i�椤
    public bool IsTutorialActive => tutorialRootPanel != null && tutorialRootPanel.activeSelf;

    void Awake()
    {
        Instance = this;
        if (tutorialRootPanel) tutorialRootPanel.SetActive(false);
    }

    public void OpenTutorial()
    {
        if (tutorialRootPanel != null) tutorialRootPanel.SetActive(true);
        CurrentStepIndex = 0;
        UpdateStepVisuals();
    }

    public void NextStep()
    {
        if (CurrentStepIndex < tutorialSteps.Count - 1)
        {
            CurrentStepIndex++;
            Debug.Log($"�оǶi�J�� {CurrentStepIndex + 1} �B");
            UpdateStepVisuals();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void UpdateStepVisuals()
    {
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i] != null)
                tutorialSteps[i].SetActive(i == CurrentStepIndex);
        }
    }

    private void CompleteTutorial()
    {
        Debug.Log("�оǧ����I");
        if (tutorialRootPanel != null) tutorialRootPanel.SetActive(false);

        if (DialogueController.Instance != null)
            DialogueController.Instance.StartInkDialogue("training_start");
    }
}