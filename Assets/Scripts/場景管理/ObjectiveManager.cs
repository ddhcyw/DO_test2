using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panelRoot;
    public TMP_Text targetText;

    public static ObjectiveManager Instance { get; private set; }

    // 1. (新增) 用來儲存 Inspector 預設輸入的文字
    private string defaultHelpMessage = "我在這裡幫助你";

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 2. (修改) 在 Awake 時儲存 Inspector 上設定好的預設文字
        //    (您需要在 Inspector 的 TargetText 元件中輸入 "我在這裡幫助你")
        if (targetText)
        {
            defaultHelpMessage = targetText.text;
        }

        // 3. (修改) 保持面板根物件是啟用的 (讓它常駐)
        if (panelRoot)
        {
            panelRoot.SetActive(true);
        }

        // (注意：您需要在 Unity 編輯器中將 ObjectivePanel 預設為 Active(true))
    }

    // 顯示新的任務指示
    public void ShowObjective(string target, string hint)
    {
        // 覆蓋掉預設的幫助訊息
        if (targetText) targetText.text = target;

    }

    // 4. (新增) 清除任務並顯示預設幫助訊息
    //    當玩家完成一個目標後，呼叫這個方法讓 UI 恢復正常
    public void ClearObjective()
    {
        // 將文本復原為預設的幫助訊息
        if (targetText) targetText.text = defaultHelpMessage;
    }
    public void HideObjective()
    {
        if (panelRoot) panelRoot.SetActive(false);
    }
}