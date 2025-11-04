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
}