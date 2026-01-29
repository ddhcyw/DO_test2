using UnityEngine;
using Spine.Unity;
using Spine;
using System;

namespace Game.Dialogue
{
    public class DialoguePortraitSpine : MonoBehaviour
    {
        [Header("DB")]
        public DialogueSpeakerDB speakerDB;

        [Header("UI References")]
        [SerializeField] private GameObject portraitRoot;     // 最外層容器（不縮放）
        [SerializeField] private RectTransform portraitHolder; // ★只縮放這個
        [SerializeField] private SkeletonGraphic uiSpine;     // Spine 本體
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool hideIfNotFound = true;

        [Header("Default")]
        [SerializeField] private string defaultSpeakerId = "MAI";
        [SerializeField] private float defaultScale = 1f;

        [Serializable]
        public class CharacterScale
        {
            public string speakerId;
            public float scale = 1f;
        }

        [Header("Per Speaker Scale")]
        [SerializeField] private CharacterScale[] characterScales;

        private void Awake()
        {
            if (!uiSpine)
                uiSpine = GetComponentInChildren<SkeletonGraphic>(true);

            if (!portraitRoot && uiSpine)
                portraitRoot = uiSpine.transform.root.gameObject;

            if (!portraitHolder && uiSpine)
                portraitHolder = uiSpine.GetComponentInParent<RectTransform>();

            if (!canvasGroup && portraitRoot)
                canvasGroup = portraitRoot.GetComponentInParent<CanvasGroup>();

            // 保險：避免 Skeleton 本體被亂縮放
            if (uiSpine)
                uiSpine.transform.localScale = Vector3.one;

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

            // 套用角色 scale（★新增）
            ApplyCharacterScale(id);

            // 換 Spine Data
            uiSpine.skeletonDataAsset = sp.spineData;
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

        private void ApplyCharacterScale(string speakerId)
        {
            if (!portraitHolder) return;

            float scale = defaultScale;

            if (characterScales != null)
            {
                foreach (var cs in characterScales)
                {
                    if (string.Equals(cs.speakerId, speakerId, StringComparison.OrdinalIgnoreCase))
                    {
                        scale = cs.scale;
                        break;
                    }
                }
            }

            portraitHolder.localScale = Vector3.one * scale;
        }

        public void ShowDefault() => SetSpeaker(defaultSpeakerId);

        public void Hide() => SetVisible(false);

        private void SetVisible(bool visible)
        {
            if (portraitRoot && !portraitRoot.activeSelf)
                portraitRoot.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (uiSpine)
                uiSpine.gameObject.SetActive(visible);
        }
    }
}
