using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ROSStereoSubscriber : MonoBehaviour
{
    // 订阅左右眼画面话题
    [Header("ROS Topics")]
    public string leftTopic = "/mujoco_server/cameras/head_zedmini_left_camera/head_zedmini_left/rgb/image_raw_compressed";
    public string rightTopic = "/mujoco_server/cameras/head_zedmini_right_camera/head_zedmini_right/rgb/image_raw_compressed";

    private ROSConnection ros;
    private Texture2D texLeft;
    private Texture2D texRight;
    private Material stereoMaterial;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        texLeft = new Texture2D(1, 1);
        texRight = new Texture2D(1, 1);
        
        // 获取当前物体上的材质球
        stereoMaterial = GetComponent<Renderer>().material;

        // 订阅左右眼画面
        ros.Subscribe<CompressedImageMsg>(leftTopic, ReceiveLeftImage);
        ros.Subscribe<CompressedImageMsg>(rightTopic, ReceiveRightImage);
    }

    void ReceiveLeftImage(CompressedImageMsg message)
    {
        // 打印日志：看看到底有没有收到，以及收到的是什么格式
        Debug.Log($"[左眼] 收到数据! 格式: {message.format}, 字节数: {message.data.Length}");
        
        // 暴力破解：不管三七二十一，直接当成 JPG/PNG 硬解压
        texLeft.LoadImage(message.data);
        stereoMaterial.SetTexture("_LeftTex", texLeft);
    }

    void ReceiveRightImage(CompressedImageMsg message)
    {
        Debug.Log($"[右眼] 收到数据! 格式: {message.format}, 字节数: {message.data.Length}");
        
        texRight.LoadImage(message.data);
        stereoMaterial.SetTexture("_RightTex", texRight);
    }
}