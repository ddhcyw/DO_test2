using UnityEngine;
using System.IO;

public class PhotoCapture : MonoBehaviour
{
    public Camera photoCamera;         // 用於拍照的攝影機
    public GameObject cmVcam;          // Cinemachine Vcam
    public KeyCode captureKey = KeyCode.C;
    public string savePath = "Screenshots";

    void Start()
    {
        if (photoCamera == null)
        {
            Debug.LogError("PhotoCamera is not assigned.");
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // 禁用 photoCamera，防止覆蓋畫面
        photoCamera.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            TakeScreenshot();
        }
    }

    void TakeScreenshot()
    {
        if (cmVcam != null)
        {
            // 獲取 CM vcam1 的 Transform
            Transform vcamTransform = cmVcam.transform;

            // 將 photoCamera 的位置與旋轉同步為 vcam1
            photoCamera.transform.position = vcamTransform.position;
            photoCamera.transform.rotation = vcamTransform.rotation;
        }

        // 啟用 photoCamera 進行拍照
        photoCamera.enabled = true;

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Screenshot_{timestamp}.png";
        string filePath = Path.Combine(savePath, fileName);

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        photoCamera.targetTexture = renderTexture;

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        // 渲染並讀取像素
        photoCamera.Render();
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // 保存圖片
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Screenshot saved to: {filePath}");

        // 清理資源
        photoCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(screenshot);

        // 拍照完成後禁用 photoCamera，繼續使用 Cinemachine 控制
        photoCamera.enabled = false;
    }
}