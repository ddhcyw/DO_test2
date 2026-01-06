using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; 
using TMPro; // 如果文字是TextMeshPro，引用這個

public class AudioSettings : MonoBehaviour
{
    [Header("音訊設定")]
    public AudioMixer mainMixer;   // 拖曳剛剛做的 MainMixer
    public string bgmParam = "BGMVolume"; // Mixer 裡暴露的參數名
    public string sfxParam = "SFXVolume";

    [Header("BGM UI")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;  // 顯示 "50" 的文字

    [Header("SFX UI")]
    public Slider sfxSlider;
    public TMP_Text sfxValueText;

    [Header("設定")]
    public float stepAmount = 10f; // 按加減按鈕一次跳多少

    void Start()
    {
        // 初始化：設定 Slider 的最大最小值 (0 ~ 100)
        bgmSlider.minValue = 0; bgmSlider.maxValue = 100;
        sfxSlider.minValue = 0; sfxSlider.maxValue = 100;

        // 讀取目前的設定 (這裡先預設 50，進階可以做讀取存檔)
        bgmSlider.value = 50;
        sfxSlider.value = 50;

        // 手動呼叫一次以更新畫面
        OnBGMChanged(bgmSlider.value);
        OnSFXChanged(sfxSlider.value);

        // 綁定 Slider 事件
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    // --- BGM 控制 ---
    public void OnBGMChanged(float value)
    {
        // 1. 更新文字
        if (bgmValueText) bgmValueText.text = value.ToString("0");

        // 2. 設定 Mixer 音量 (將 0-100 轉換為 分貝 -80~0)
        float db = (value <= 0.001f) ? -80f : Mathf.Log10(value / 100f) * 20f;
        mainMixer.SetFloat(bgmParam, db);
    }

    public void OnBGMPlus() { bgmSlider.value += stepAmount; }
    public void OnBGMMinus() { bgmSlider.value -= stepAmount; }

    // --- SFX 控制 ---
    public void OnSFXChanged(float value)
    {
        // 1. 更新文字
        if (sfxValueText) sfxValueText.text = value.ToString("0");

        // 2. 設定 Mixer 音量
        float db = (value <= 0.001f) ? -80f : Mathf.Log10(value / 100f) * 20f;
        mainMixer.SetFloat(sfxParam, db);
    }

    public void OnSFXPlus() { sfxSlider.value += stepAmount; }
    public void OnSFXMinus() { sfxSlider.value -= stepAmount; }

    // --- 返回按鈕 ---
    public void CloseAudioPanel()
    {
        gameObject.SetActive(false); // 關閉自己
        // 如果有上一層主選單，要在這裡把它打開，例如：
        // settingsMainMenu.SetActive(true);
    }
}