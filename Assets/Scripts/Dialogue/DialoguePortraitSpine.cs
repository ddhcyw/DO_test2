using UnityEngine;
using Spine;
using Spine.Unity;

public class DialoguePortraitSpine : MonoBehaviour
{
    public DialogueSpeakerDB speakerDB;
    public SkeletonGraphic graphic;

    string currentSpeakerId = "";

    void Awake()
    {
        if (!graphic) graphic = GetComponentInChildren<SkeletonGraphic>(true);
        gameObject.SetActive(false);
    }

    public void SetSpeaker(string speakerId)
    {
        if (string.IsNullOrEmpty(speakerId))
        {
            Hide();
            return;
        }

        // 同一個人就不要重複切換（避免每句都 Initialize）
        if (speakerId == currentSpeakerId) return;
        currentSpeakerId = speakerId;

        var sp = speakerDB != null ? speakerDB.Get(speakerId) : null;
        if (sp == null || sp.spine == null)
        {
            Debug.LogWarning($"[Portrait] Speaker not found in DB: {speakerId}");
            Hide();
            return;
        }

        Show(sp);
    }

    void Show(DialogueSpeakerDB.Speaker sp)
    {
        if (!graphic) return;

        graphic.skeletonDataAsset = sp.spine;
        graphic.Initialize(true);

        var sk = graphic.Skeleton;

        // skin 可有可無
        if (!string.IsNullOrEmpty(sp.skin))
        {
            var found = sk.Data.FindSkin(sp.skin);
            if (found != null)
            {
                sk.SetSkin(found);
                sk.SetSlotsToSetupPose();
            }
        }

        // 播動畫（你目前 worm 的案例是用動畫當顏色，這裡也是同理）
        if (!string.IsNullOrEmpty(sp.anim) && sk.Data.FindAnimation(sp.anim) != null)
        {
            graphic.AnimationState.SetAnimation(0, sp.anim, sp.loop);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentSpeakerId = "";
        gameObject.SetActive(false);
    }
}
