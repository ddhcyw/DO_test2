using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // 為了使用 Coroutine

public class CameraSimulation : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI 元件設定")]
    public Button takePictureButton;
    public Button photoEffectButton;

    // === 內部變數 ===
    private Item finalPhotoItemToAdd;
    private Sprite finalCollectedPhotoSpriteToAdd;
    private bool isShowingPhotoEffect = false;
    private Canvas rootCanvas; // 儲存根 Canvas

    void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (photoEffectButton != null)
        {
            photoEffectButton.gameObject.SetActive(false);
        }

        // 修正「點擊穿透」
        if (takePictureButton != null)
        {
            takePictureButton.interactable = false;
            StartCoroutine(EnableButtonAfterDelay());
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
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            if (TutorialManager.Instance.CurrentStepIndex == TutorialManager.Instance.takePhotoStepIndex)
            {
                Debug.Log("教學：玩家成功按下快門！進入下一步");
                TutorialManager.Instance.NextStep();
            }
        }
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
        if (photoEffectButton != null && rootCanvas != null)
        {
            isShowingPhotoEffect = true;

            //將 photoEffectButton 從觀景窗「拔」出來
            //並將它設為 rootCanvas 的子物件，這樣它才能全螢幕
            photoEffectButton.transform.SetParent(rootCanvas.transform, false);

            //取得 RectTransform 並強制設為全螢幕
            RectTransform rt = photoEffectButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); // 左下角
            rt.anchorMax = new Vector2(1, 1); // 右上角
            rt.offsetMin = Vector2.zero;      // Left, Bottom
            rt.offsetMax = Vector2.zero;      // Right, Top

            //設定示意圖並顯示
            photoEffectButton.image.sprite = spriteToShow;
            photoEffectButton.gameObject.SetActive(true);

            //隱藏原本的「觀景窗」UI (但不要銷毀，CloseSimulation 會處理)
            //我們透過禁用 Image 和 Button 來隱藏它，而不是 SetActive(false)
            takePictureButton.gameObject.SetActive(false);
            GetComponent<Image>().enabled = false; // 隱藏觀景窗背景
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
        //銷毀「全螢幕示意圖」物件
        if (photoEffectButton != null)
        {
            Destroy(photoEffectButton.gameObject);
        }
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