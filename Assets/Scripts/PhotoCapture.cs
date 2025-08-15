using UnityEngine;
using System.IO;

public class PhotoCapture : MonoBehaviour
{
    public Camera photoCamera;         // �Ω��Ӫ���v��
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
            // CM vcam1 Transform
            Transform vcamTransform = cmVcam.transform;

            // photoCamera 位置與旋轉對齊 vcam1
            photoCamera.transform.position = vcamTransform.position;
            photoCamera.transform.rotation = vcamTransform.rotation;
        }

        // photoCamera
        photoCamera.enabled = true;

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Screenshot_{timestamp}.png";
        string filePath = Path.Combine(savePath, fileName);

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        photoCamera.targetTexture = renderTexture;

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        // 擷取畫面
        photoCamera.Render();
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // 儲存截圖
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Screenshot saved to: {filePath}");

        // �M�z�귽
        photoCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(screenshot);

        // ��ӧ�����T�� photoCamera�A�~��ϥ� Cinemachine ����
        photoCamera.enabled = false;
    }
}