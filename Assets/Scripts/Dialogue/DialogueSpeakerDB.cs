using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(menuName = "Dialogue/Speaker DB")]
public class DialogueSpeakerDB : ScriptableObject
{
    [System.Serializable]
    public class Speaker
    {
        public string id;                 // Ink 的 who，例如 "MAI" "NPC1"
        public SkeletonDataAsset spine;   // 對應角色 Spine 資料
        public string skin = "default";   // 沒有 skin 就留 default
        public string anim = "idle";      // 角色在對話顯示時播的動畫
        public bool loop = true;
    }

    public Speaker[] speakers;

    public Speaker Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var s in speakers)
            if (s.id == id) return s;
        return null;
    }
}
