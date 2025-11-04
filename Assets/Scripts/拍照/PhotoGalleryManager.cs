using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhotoGalleryManager : MonoBehaviour
{
    public static PhotoGalleryManager Instance { get; private set; }

    // 1. (設定) 主畫面左上角的照片欄位 Image 元件陣列
    //    請將您的 3 個黑白照片 Image 依序拖曳到這裡
    public Image[] photoCollectionSlots;

    // 2. (設定) 初始的黑白照片 Sprite (如果收集欄位預設是空的，則設為 null)
    public Sprite defaultGrayscalePhoto;

    // 內部數據，記錄哪些照片已被收集
    private List<Sprite> collectedPhotoSprites = new List<Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 如果此 Manager 需跨場景保留則保留

            // 初始化收集欄位，全部顯示黑白或空白
            UpdateGalleryUI();
        }
    }

    // 將收集到的彩色照片 Sprite 加入收集欄
    public void AddCollectedPhoto(Sprite photoSprite)
    {
        if (photoSprite == null)
        {
            Debug.LogWarning("嘗試加入空的照片 Sprite 到照片收集欄。");
            return;
        }
        if (collectedPhotoSprites.Contains(photoSprite))
        {
            Debug.Log("照片 " + photoSprite.name + " 已經在收集欄中了，不再重複加入。");
            return;
        }

        collectedPhotoSprites.Add(photoSprite);
        UpdateGalleryUI(); // 更新 UI 顯示
    }

    // 更新 UI 顯示 (將黑白照片替換成彩色)
    private void UpdateGalleryUI()
    {
        for (int i = 0; i < photoCollectionSlots.Length; i++)
        {
            if (i < collectedPhotoSprites.Count)
            {
                // 如果有收集到的照片，就顯示彩色照片
                photoCollectionSlots[i].sprite = collectedPhotoSprites[i];
                photoCollectionSlots[i].color = Color.white; // 確保顏色是全白 (不透明)
            }
            else
            {
                // 否則顯示黑白預設圖 (或空白)
                photoCollectionSlots[i].sprite = defaultGrayscalePhoto;
                if (defaultGrayscalePhoto == null)
                {
                    photoCollectionSlots[i].color = Color.clear; // 如果沒有預設圖就透明
                }
                else
                {
                    photoCollectionSlots[i].color = Color.white;
                }
            }
        }
    }
}