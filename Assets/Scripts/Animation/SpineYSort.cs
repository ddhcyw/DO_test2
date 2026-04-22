using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class SpineYSort : MonoBehaviour
{
    [Header("層級設定")]
    public int sortingOffset = 0;
    public float baseLine = 0f;

    private SkeletonAnimation skeleton;

    void Awake()
    {
        skeleton = GetComponent<SkeletonAnimation>();
    }

    void LateUpdate()
    {
        // 用父物件的世界Y座標排序（因為SpineYSort通常掛在子物件）
        float worldY = transform.parent != null
            ? transform.parent.position.y
            : transform.position.y;

        float sortValue = worldY + baseLine;
        int order = sortingOffset - Mathf.RoundToInt(sortValue * 100f);
        skeleton.GetComponent<MeshRenderer>().sortingOrder = order;
    }
}
