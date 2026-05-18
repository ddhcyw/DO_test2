using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("過場影格（依序放入 frame0 ~ frame4）")]
    [SerializeField] private Sprite[] frames;

    [Header("UI 元件")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private Image transitionImage;

    [Header("每格停留秒數（5格×1秒=5秒）")]
    [SerializeField] private float frameInterval = 1f;

    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (transitionCanvas != null)
        {
            // If canvas is not a child of this object, reparent it so DontDestroyOnLoad covers it
            if (transitionCanvas.transform.parent != transform)
                transitionCanvas.transform.SetParent(transform, false);
            transitionCanvas.gameObject.SetActive(false);
        }
        else
            Debug.LogError("[SceneTransitionManager] transitionCanvas 未指定，請在 Inspector 拖入！");
    }

    public void TransitionToScene(string sceneName)
    {
        Debug.Log($"[STM] TransitionToScene called: {sceneName}, isTransitioning={isTransitioning}");
        if (isTransitioning) return;
        StartCoroutine(DoTransition(sceneName));
    }

    private IEnumerator DoTransition(string sceneName)
    {
        isTransitioning = true;
        Debug.Log($"[STM] DoTransition 開始, canvas={transitionCanvas}, frames={frames?.Length}");
        if (transitionCanvas == null)
        {
            Debug.LogError("[SceneTransitionManager] transitionCanvas 是 null，請在 Inspector 拖入 Canvas！直接載入場景。");
            yield return SceneManager.LoadSceneAsync(sceneName);
            isTransitioning = false;
            yield break;
        }
        transitionCanvas.gameObject.SetActive(true);
        Debug.Log("[STM] Canvas 已啟用，開始播放影格");

        // 離開：正向播放 0→4
        yield return StartCoroutine(PlayFrames(forward: true));

        // 載入新場景
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 進入：反向播放 4→0
        yield return StartCoroutine(PlayFrames(forward: false));

        transitionCanvas.gameObject.SetActive(false);
        isTransitioning = false;
    }

    private IEnumerator PlayFrames(bool forward)
    {
        int start = forward ? 0 : frames.Length - 1;
        int end   = forward ? frames.Length : -1;
        int step  = forward ? 1 : -1;

        for (int i = start; i != end; i += step)
        {
            transitionImage.sprite = frames[i];
            yield return new WaitForSeconds(frameInterval);
        }
    }
}
