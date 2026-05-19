using UnityEngine;
using UnityEngine.EventSystems;

public class ClueObjectInteract : MonoBehaviour
{
    [Header("要播放的 Ink 節點名稱")]
    public string inkKnotName;

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (GameFlow.Instance == null) return;
        if (GameFlow.Instance.CurrentState == GameFlow.GameState.Talking) return;

        Debug.Log($"點擊了線索物件，觸發節點: {inkKnotName}");

        GameFlow.Instance.StartDialogue(inkKnotName);
    }
}
