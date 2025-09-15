using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int sortingOffset = 0;
    SpriteRenderer sr;
    void Awake(){ sr = GetComponent<SpriteRenderer>(); }
    void LateUpdate(){ sr.sortingOrder = sortingOffset - Mathf.RoundToInt(transform.position.y * 100f); }
}
