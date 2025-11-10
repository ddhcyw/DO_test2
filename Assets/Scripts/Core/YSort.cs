using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    [Header("層級設定")]
    public int sortingOffset = 0;
    [Tooltip("調整角色腳底基準（越大越往上貼）")]
    public float baseLine = 0f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // 根據Y座標排序，越下方越前面
        float sortValue = transform.position.y + baseLine;
        sr.sortingOrder = sortingOffset - Mathf.RoundToInt(sortValue * 100f);
    }
}
