using UnityEngine;
using Spine.Unity;
using Spine;

namespace Game.Dialogue
{
    public class DialoguePortraitSpine : MonoBehaviour
    {
        [Header("DB")]
        public DialogueSpeakerDB speakerDB;

        [Header("UI References")]
        [SerializeField] private GameObject portraitRoot; // 建議指到 PortraitRoot（容器）
        [SerializeField] private SkeletonGraphic uiSpine; // 指到 PortraitSpine 上的 SkeletonGraphic
        [SerializeField] private CanvasGroup canvasGroup; // 可選
        [SerializeField] private bool hideIfNotFound = true;

        [Header("Default")]
        [SerializeField] private string defaultSpeakerId = "MAI";

        private void Awake()
        {
            if (!uiSpine) uiSpine = GetComponentInChildren<SkeletonGraphic>(true);

            // portraitRoot：優先用你手動指定的 PortraitRoot；沒指定才退回用 uiSpine 的 GameObject
            if (!portraitRoot && uiSpine) portraitRoot = uiSpine.gameObject;

            // CanvasGroup：可選，有就用，沒有也不影響顯示
            if (!canvasGroup && portraitRoot) canvasGroup = portraitRoot.GetComponentInParent<CanvasGroup>();

            SetVisible(false);
        }

        public void SetSpeaker(string id)
        {
            if (!uiSpine)
            {
                SetVisible(false);
                return;
            }

            if (speakerDB == null || !speakerDB.TryGet(id, out var sp) || sp.spineData == null)
            {
                if (hideIfNotFound) SetVisible(false);
                return;
            }

            SetVisible(true);

            // Spine-Unity 3.8：用 skeletonDataAsset 欄位，不是 SkeletonDataAsset 屬性
            uiSpine.skeletonDataAsset = sp.spineData;

            // 重新初始化
            uiSpine.Initialize(true);

            // Skin（可選）
            if (!string.IsNullOrWhiteSpace(sp.skin) && uiSpine.Skeleton != null)
            {
                var sk = uiSpine.Skeleton;
                if (sk.Data.FindSkin(sp.skin) != null)
                {
                    sk.SetSkin(sp.skin);
                    sk.SetSlotsToSetupPose();
                    uiSpine.Update(0);
                }
            }

            // Animation（可選）
            var animName = string.IsNullOrWhiteSpace(sp.anim) ? "idle" : sp.anim;
            if (uiSpine.AnimationState != null && uiSpine.SkeletonData != null)
            {
                if (uiSpine.SkeletonData.FindAnimation(animName) != null)
                {
                    uiSpine.AnimationState.SetAnimation(0, animName, sp.loop);
                }
            }
        }

        public void ShowDefault() => SetSpeaker(defaultSpeakerId);

        public void Hide() => SetVisible(false);

        private void SetVisible(bool visible)
        {
            // 1) 容器永遠保持 active，避免把自己/父物件關掉後無法再被喚醒
            if (portraitRoot && !portraitRoot.activeSelf)
                portraitRoot.SetActive(true);

            // 2) 用 CanvasGroup 控制顯示
            if (canvasGroup)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            // 3) 只開關 Spine 物件（被隱藏時就不渲染）
            if (uiSpine)
                uiSpine.gameObject.SetActive(visible);
        }

    }
}
