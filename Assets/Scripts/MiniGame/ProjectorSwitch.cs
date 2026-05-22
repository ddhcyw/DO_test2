using UnityEngine;

public class ProjectorSwitch : MonoBehaviour
{
    [Header("�n���������� (�쥻����v�e��)")]
    public GameObject objectToHide;

    [Header("�n�X�{������ (�t��/�J�f)")]
    public GameObject objectToShow;

    [Header("����")]
    public AudioSource audioSource;

    [Header("鏡頭平移設定")]
    public CameraFollow cameraFollow;
    public float panTravelTime = 1.2f;  // 移動到暗門所需秒數
    public float panStayTime  = 2.0f;  // 停留在暗門的秒數

    void OnMouseDown()
    {
        if (GameFlow.Instance != null && GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
            return;

        PerformSwitch();
    }

    void PerformSwitch()
    {
        if (objectToHide != null)
            objectToHide.SetActive(false);

        if (objectToShow != null)
            objectToShow.SetActive(true);

        if (audioSource != null)
            audioSource.Play();

        // 鏡頭平移：移到暗門停留後自動回到主角
        if (cameraFollow != null && objectToShow != null)
            cameraFollow.PanToTarget(objectToShow.transform, panTravelTime, panStayTime);

        gameObject.SetActive(false);
        Debug.Log("投影機已關閉，暗門出現！");
    }
}