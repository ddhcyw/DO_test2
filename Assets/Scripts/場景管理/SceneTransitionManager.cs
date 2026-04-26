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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        transitionCanvas.gameObject.SetActive(false);
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(sceneName));
    }

    private IEnumerator DoTransition(string sceneName)
    {
        isTransitioning = true;
        transitionCanvas.gameObject.SetActive(true);

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
