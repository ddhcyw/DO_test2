using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace Game.Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Speaker DB", fileName = "SpeakerDB")]
    public class DialogueSpeakerDB : ScriptableObject
    {
        [Serializable]
        public class Speaker
        {
            public string id = "MAI";

            [Header("Spine (UI)")]
            public SkeletonDataAsset spineData;

            [Header("Optional")]
            public string skin;          // 留空 = 不換 skin
            public string anim = "idle"; // 預設動畫
            public bool loop = true;
        }


        public List<Speaker> speakers = new List<Speaker>();

        public bool TryGet(string id, out Speaker speaker)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                speaker = null;
                return false;
            }

            for (int i = 0; i < speakers.Count; i++)
            {
                var s = speakers[i];
                if (s != null && string.Equals(s.id, id, StringComparison.OrdinalIgnoreCase))
                {
                    speaker = s;
                    return true;
                }
            }

            speaker = null;
            return false;
        }
    }
}
