using System.Collections;
using UnityEngine;

public class MAIHelpPanelAppear : MonoBehaviour
{
    RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    public void PlayAppear()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PunchScale());
    }

    IEnumerator PunchScale()
    {
        float[] scales = { 1.15f, 0.92f, 1.05f, 1.0f };
        float[] times  = { 0.12f, 0.10f, 0.08f, 0.06f };

        for (int i = 0; i < scales.Length; i++)
        {
            float start = rt.localScale.x;
            float end = scales[i];
            float t = 0, dur = times[i];
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(start, end, t / dur);
                rt.localScale = Vector3.one * s;
                yield return null;
            }
            rt.localScale = Vector3.one * end;
        }
    }
}
