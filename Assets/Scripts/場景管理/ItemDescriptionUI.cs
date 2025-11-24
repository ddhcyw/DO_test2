using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionUI : MonoBehaviour
{
    // 單例模式，讓格子可以直接找到它
    public static ItemDescriptionUI Instance { get; private set; }

    [Header("UI 元件")]
    public GameObject contentRoot;   // 用來控制顯示/隱藏 (如果沒選道具就隱藏)
    public Image itemDisplayImage;   // 顯示道具大圖
    public TMP_Text itemNameText;    // 顯示道具名稱
    public TMP_Text itemDescText;    // 顯示道具功能描述

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 遊戲開始時先清空/隱藏資訊
        ClearDescription();
    }

    // 顯示道具資訊
    public void ShowDescription(Item item)
    {
        if (item == null) return;

        if (contentRoot) contentRoot.SetActive(true);

        if (itemDisplayImage)
        {
            itemDisplayImage.sprite = item.icon;
            itemDisplayImage.gameObject.SetActive(item.icon != null);
        }

        if (itemNameText) itemNameText.text = item.name;
        if (itemDescText) itemDescText.text = item.description;
    }

    // 清空資訊
    public void ClearDescription()
    {
        if (contentRoot) contentRoot.SetActive(false);

        if (itemNameText) itemNameText.text = "";
        if (itemDescText) itemDescText.text = "";
    }
}