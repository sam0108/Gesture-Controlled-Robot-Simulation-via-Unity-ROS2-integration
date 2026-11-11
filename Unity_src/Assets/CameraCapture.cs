using UnityEngine;
using UnityEngine.UI; // 如果使用 UGUI
using System.Collections;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;

public class CameraCapture : MonoBehaviour
{
    public RawImage displayImage; // 拖入畫面中的 Raw Image

    private ROSConnection ros;
    public string topicName = "/raw_image";

    private WebCamTexture webcamTexture;
    private Texture2D texture2D;

    [Header("影像設定")]
    public int width = 640;
    public int height = 480;
    public int fps = 30;

    void Start()
    {
        // 檢查權限 (Windows/Android/iOS 適用)
        StartCoroutine(RequestCameraPermission());

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        // 【修正】如果相機還沒初始化完成，就直接跳過這一幀
        if (webcamTexture == null) 
        {
            return;
        }

        if (webcamTexture.didUpdateThisFrame)
        {
            PublishImage();
        }
    }

    void PublishImage(){
        // 確保 texture2D 大小正確
        if (texture2D.width != webcamTexture.width || texture2D.height != webcamTexture.height)
        {
            texture2D.Reinitialize(webcamTexture.width, webcamTexture.height);
        }

        texture2D.SetPixels(webcamTexture.GetPixels());
        texture2D.Apply();

        // 將資料轉為 byte[]
        byte[] imageData = texture2D.GetRawTextureData();

        var msg = new ImageMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg { frame_id = "webcam_link" },
            height = (uint)texture2D.height,
            width = (uint)texture2D.width,
            encoding = "rgb8", // 這裡發布的是 RGB
            step = (uint)(texture2D.width * 3),
            data = imageData
        };

        ros.Publish(topicName, msg);
    }

    // void PublishImage()
    // {
    //     // 將 Webcam 像素拷貝到 Texture2D
    //     texture2D.SetPixels(webcamTexture.GetPixels());
    //     texture2D.Apply();

    //     byte[] imageData = texture2D.GetRawTextureData();

    //     var msg = new ImageMsg
    //     {
    //         header = new RosMessageTypes.Std.HeaderMsg { frame_id = "webcam_link" },
    //         height = (uint)height,
    //         width = (uint)width,
    //         encoding = "rgb8",
    //         step = (uint)(width * 3),
    //         data = imageData
    //     };

    //     ros.Publish(topicName, msg);
    // }

    IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            StartWebCam();
        }
        else
        {
            Debug.LogError("相機權限被拒絕");
        }
    }

    void StartWebCam(){
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) return;

        // 1. 初始化 WebCamTexture
        webcamTexture = new WebCamTexture(devices[0].name, width, height, fps);
        webcamTexture.Play();

        // 2. 【關鍵修正】重新建立符合相機實際輸出的 Texture2D
        // 有些相機會忽略你要求的 640x480，而給出它最接近的解析度
        texture2D = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        
        displayImage.texture = webcamTexture;
        // Debug.Log($"相機實際解析度: {webcamTexture.width}x{webcamTexture.height}");
    }

    void OnDisable()
    {
        // 停止相機以釋放資源
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}