using UnityEngine;
using Spine;
using Spine.Unity;

public class DialoguePortraitSpine : MonoBehaviour
{
    public DialogueSpeakerDB speakerDB;
    public SkeletonGraphic graphic;

    private string currentSpeakerId = "";

    void Awake()
    {
        if (!graphic)
            graphic = GetComponentInChildren<SkeletonGraphic>(true);

        gameObject.SetActive(false);
    }

    public void SetSpeaker(string speakerId)
    {
        if (string.IsNullOrEmpty(speakerId))
        {
            Hide();
            return;
        }

        if (speakerId == currentSpeakerId)
            return;

        currentSpeakerId = speakerId;

        if (speakerDB == null)
        {
            Debug.LogWarning("[Portrait] SpeakerDB not assigned");
            Hide();
            return;
        }

        var sp = speakerDB.Get(speakerId);
        if (sp == null || sp.spine == null)
        {
            Debug.LogWarning($"[Portrait] Speaker not found: {speakerId}");
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

        if (!string.IsNullOrEmpty(sp.skin))
        {
            var skin = sk.Data.FindSkin(sp.skin);
            if (skin != null)
            {
                sk.SetSkin(skin);
                sk.SetSlotsToSetupPose();
            }
        }

        if (!string.IsNullOrEmpty(sp.anim) &&
            sk.Data.FindAnimation(sp.anim) != null)
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
