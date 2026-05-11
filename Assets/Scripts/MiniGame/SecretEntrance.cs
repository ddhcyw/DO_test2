using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SecretEntrance : MonoBehaviour
{
    [Header("UI 設定")]
    public GameObject puzzlePanel;
    public Image leverImage;
    public Image leverDownSprite;

    [Header("場景設定")]
    public string baseSceneName = "(2D)作品集偷偷的基地 1";

    void OnMouseDown()
    {
        if (GameFlow.Instance != null && GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
            return;

        OpenPanel();
    }

    public void OpenPanel()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);
    }

    public void OnClickCorrectLever()
    {
        StartCoroutine(PullLeverProcess());
    }

    IEnumerator PullLeverProcess()
    {
        if (leverImage != null && leverDownSprite != null)
        {
            leverImage.sprite = leverDownSprite.sprite;
            leverImage.SetNativeSize();
            leverImage.rectTransform.anchoredPosition += new Vector2(0, -105f);
        }

        Debug.Log("繩子已拉下！準備傳送...");

        yield return new WaitForSeconds(0.8f);

        var stm = SceneTransitionManager.Instance;
        if (stm == null) stm = FindObjectOfType<SceneTransitionManager>();

        if (stm != null)
            stm.TransitionToScene(baseSceneName);
        else
            SceneManager.LoadScene(baseSceneName);
    }

    public void OnClickWrongLever()
    {
        Debug.Log("這條繩子拉下去什麼都沒發生...");
    }
}
