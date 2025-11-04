using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // 為了使用 Coroutine

public class CameraSimulation : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI 元件設定")]
    public Button takePictureButton;
    public Button photoEffectButton;

    // === 內部變數 (儲存「即將」要用的照片) ===
    private Item finalPhotoItemToAdd;
    private Sprite finalCollectedPhotoSpriteToAdd;
    private bool isShowingPhotoEffect = false; // 狀態旗標

    void Start()
    {
        if (photoEffectButton != null)
        {
            photoEffectButton.gameObject.SetActive(false);
        }

        // 修正「點擊穿透」問題
        if (takePictureButton != null)
        {
            takePictureButton.interactable = false; // 先禁用
            StartCoroutine(EnableButtonAfterDelay()); // 延遲啟用
        }

        if (photoEffectButton != null)
        {
            photoEffectButton.onClick.AddListener(ProcessAndCloseSimulation);
        }
    }

    // Coroutine 延遲啟用按鈕
    private IEnumerator EnableButtonAfterDelay()
    {
        yield return null; // 等待一幀

        if (takePictureButton != null)
        {
            takePictureButton.interactable = true;
            takePictureButton.onClick.AddListener(OnTakePhotoClick);
        }
    }

    private void OnTakePhotoClick()
    {
        // 從 UI 觀景窗的位置，向 2D 世界發射一條射線
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(transform.position), Vector2.zero);
        Sprite spriteToShow;

        // 檢查是否射線有打到東西，且該物體有 Photographable 腳本
        if (hit.collider != null && hit.collider.TryGetComponent<Photographable>(out Photographable target))
        {
            // 拍到了特殊物體！
            Debug.Log("拍到了：" + target.name);
            finalPhotoItemToAdd = target.photoItemToGive;
            finalCollectedPhotoSpriteToAdd = target.collectedPhotoSprite;
            spriteToShow = target.photoEffectSprite;

            if (finalPhotoItemToAdd == null || finalCollectedPhotoSpriteToAdd == null || spriteToShow == null)
            {
                Debug.LogError("物體 " + target.name + " 上的 Photographable 腳本欄位未設定完整！");
                return; // 欄位不完整，也取消拍照
            }
        }
        else
        {
            // *** 核心修正：拍到空景 ***
            Debug.Log("未拍到任何可拍攝物體，取消拍照。");
            return; // *** 停止執行，不顯示示意圖，也不給照片 ***
        }

        // --- 顯示示意圖 (只有在成功拍到時才會執行到這裡) ---
        if (photoEffectButton != null)
        {
            // 將示意圖 Sprite 設置到按鈕的 Image 上
            photoEffectButton.image.sprite = spriteToShow;
            photoEffectButton.gameObject.SetActive(true);
            isShowingPhotoEffect = true; // 設定旗標

            // 隱藏原本的「拍照」按鈕
            takePictureButton.gameObject.SetActive(false);
        }
        else
        {
            // Failsafe: 萬一沒有設定 photoEffectButton，直接處理照片
            ProcessAndCloseSimulation();
        }
    }

    private void ProcessAndCloseSimulation()
    {
        isShowingPhotoEffect = false; // 重設旗標

        if (finalPhotoItemToAdd == null || finalCollectedPhotoSpriteToAdd == null)
        {
            Debug.LogError("要新增的照片 Item 或 Collected Photo Sprite 是 null！");
            CloseSimulation();
            return;
        }

        // 將照片加入背包
        bool wasAdded = InventoryManager.Instance.Add(finalPhotoItemToAdd);

        if (wasAdded)
        {
            Debug.Log("成功拍到照片，已加入背包！");
            // 呼叫 PhotoGalleryManager 加入彩色照片
            PhotoGalleryManager.Instance.AddCollectedPhoto(finalCollectedPhotoSpriteToAdd);
        }
        else
        {
            Debug.LogWarning("背包已滿，照片無法加入！");
        }

        CloseSimulation();
    }

    private void CloseSimulation()
    {
        Destroy(gameObject);
    }


    // === UI 拖曳功能 ===
    private Vector2 pointerOffset;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isShowingPhotoEffect) return; // 顯示示意圖時不能拖曳

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            Vector2 newPosition = localPointerPosition - pointerOffset;
            this.GetComponent<RectTransform>().localPosition = newPosition;
        }
    }
}