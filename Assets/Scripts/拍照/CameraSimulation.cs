using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // 為了使用 Coroutine

public class CameraSimulation : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI 元件設定")]
    public Button takePictureButton;
    public Button photoEffectButton;
    [Header("預設圖片設定")] 
    public Sprite emptyPhotoEffectSprite; // 拍到空景時要顯示的全螢幕預覽圖

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
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(transform.position), Vector2.zero);
        Sprite spriteToShow;

        if (hit.collider != null && hit.collider.TryGetComponent<Photographable>(out Photographable target))
        {
            // === 情況 A：成功拍到有能量的物體 ===
            Debug.Log("拍到了：" + target.name);
            finalPhotoItemToAdd = target.photoItemToGive;
            finalCollectedPhotoSpriteToAdd = target.collectedPhotoSprite;
            spriteToShow = target.photoEffectSprite;

            if (finalPhotoItemToAdd == null || finalCollectedPhotoSpriteToAdd == null || spriteToShow == null)
            {
                Debug.LogError("物體 " + target.name + " 上的 Photographable 腳本欄位未設定完整！");
                return;
            }
        }
        else
        {
            // === 情況 B：拍到空景 (沒有能量) ===
            Debug.Log("未拍到特殊物體，顯示空景照片。");

            // 這裡清空原本要加進圖鑑的資料，代表「這張照片沒能量」
            finalPhotoItemToAdd = null; // 如果你不想放進背包，這行可以保持為 null
            finalCollectedPhotoSpriteToAdd = null; // 絕對不要加進圖鑑

            // 設定全螢幕要顯示的「無能量照片」
            spriteToShow = emptyPhotoEffectSprite;

            if (spriteToShow == null)
            {
                Debug.LogWarning("你還沒有在 Inspector 設定 emptyPhotoEffectSprite！");
                return;
            }
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

        // 如果玩家拍到的是「有能量」的照片，才執行加入背包和圖鑑的動作
        if (finalPhotoItemToAdd != null && finalCollectedPhotoSpriteToAdd != null)
        {
            bool wasAdded = InventoryManager.Instance.Add(finalPhotoItemToAdd);

            if (wasAdded)
            {
                Debug.Log("成功拍到有能量的照片，已加入背包與圖鑑！");
                PhotoGalleryManager.Instance.AddCollectedPhoto(finalCollectedPhotoSpriteToAdd);
            }
            else
            {
                Debug.LogWarning("背包已滿，照片無法加入！");
            }
        }
        else
        {
            // 這是拍到空景的狀況
            Debug.Log("這是一張沒有能量的照片，不加入圖鑑。");
            // 如果你有設定 emptyPhotoItem，也可以在這裡寫加入背包的邏輯
        }

        // 最後一定要關閉並銷毀相機
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