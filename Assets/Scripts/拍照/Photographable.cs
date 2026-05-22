using UnityEngine;

//掛到拍攝物
public class Photographable : MonoBehaviour
{
    [Header("拍照後給予的數據")]
    // 1. 拍下此物體後，要加入背包的「照片」Item
    public Item photoItemToGive;

    // 2. 拍下此物體後，要顯示的「示意圖」Sprite
    public Sprite photoEffectSprite;

    // 3. 拍下此物體後，要更新到「照片收集欄」的 Sprite
    public Sprite collectedPhotoSprite;

    [Header("劇情鎖定設定")]
    [Tooltip("如果勾選，這張照片必須在特定劇情後才能拍到。否則會顯示空景。")]
    public bool requireQuest = false;

    [Tooltip("填入要檢查的 PlayerPrefs Key")]
    public string questKey = "LeahPhotoUnlocked";

    // 提供給 CameraSimulation 檢查用的功能
    public bool IsUnlocked()
    {
        // 如果不需要解任務，直接回傳 true (可拍)
        if (!requireQuest) return true;

        // 如果需要解任務，但沒有填寫 Key，防呆回傳 false (不可拍)
        if (string.IsNullOrEmpty(questKey)) return false;

        // 檢查 PlayerPrefs 中這個 Key 的值是否為 1 (1 代表已完成)
        return PlayerPrefs.GetInt(questKey, 0) == 1;
    }
}