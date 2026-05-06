using UnityEngine;
using UnityEngine.UI; 

public class ClueVisualizer : MonoBehaviour
{
    [Header("燈的圖片元件 (依序為 PC, Copy, Canvas)")]
    public Image[] clueLights;

    public Color inactiveColor = Color.white;
    public Color activeColor = Color.yellow;

    private void Start()
    {
        RefreshLights();
    }

    // 重新檢查所有線索並更新顏色
    public void RefreshLights()
    {
        if (GameFlow.Instance == null) return;

        // 檢查順序與 HasAllBaseClues 的邏輯對應
        UpdateLight(0, "clue_canvas");
        UpdateLight(1, "clue_copy_machine");
        UpdateLight(2, "clue_pc");
    }

    private void UpdateLight(int index, string clueID)
    {
        if (index >= clueLights.Length || clueLights[index] == null) return;

        bool hasIt = GameFlow.Instance.HasClue(clueID);
        clueLights[index].color = hasIt ? activeColor : inactiveColor;
    }
}