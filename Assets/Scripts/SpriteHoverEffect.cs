using UnityEngine;

public class SpriteHoverEffect : MonoBehaviour
{
    // 普通狀態的
    public GameObject normalStateObject;
    public GameObject normalDioObject;
    // 滑鼠懸停時要顯示
    public GameObject hoverStateObject;
    public GameObject hoverDioObject;

    void Start()
    {
        // 確保遊戲一開始時的狀態是正確的
        if (normalStateObject != null) normalStateObject.SetActive(true);
        if (hoverStateObject != null) hoverStateObject.SetActive(false);
        if (normalDioObject != null) normalDioObject.SetActive(true);
        if (hoverDioObject != null) hoverDioObject.SetActive(false);
    }

    // 當滑鼠指標「進入」這個物件的 Collider 範圍時，Unity 會自動呼叫此函式
    private void OnMouseEnter()
    {
        if (normalStateObject != null) normalStateObject.SetActive(false);
        if (hoverStateObject != null) hoverStateObject.SetActive(true);
        if (normalDioObject != null) normalDioObject.SetActive(false);
        if (hoverDioObject != null) hoverDioObject.SetActive(true);
    }

    // 當滑鼠指標「離開」這個物件的 Collider 範圍時，Unity 會自動呼叫此函式
    private void OnMouseExit()
    {
        if (normalStateObject != null) normalStateObject.SetActive(true);
        if (hoverStateObject != null) hoverStateObject.SetActive(false);
        if (normalDioObject != null) normalDioObject.SetActive(true);
        if (hoverDioObject != null) hoverDioObject.SetActive(false);
    }
}