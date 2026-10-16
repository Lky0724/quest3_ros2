using UnityEngine;
using TMPro; // 使用 TextMeshPro UI
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;

public class DynamicROSConnection : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField ipInputField; // 拖入你的输入框
    public Button connectButton;        // 拖入你的按钮

    [Header("ROS Connections")]
    [Tooltip("ROSConnection for the image/video endpoint (e.g. port 10000).")]
    public ROSConnection imageConnection;
    [Tooltip("ROSConnection for the head-control endpoint (e.g. port 10001).")]
    public ROSConnection controlConnection;

    private void Start()
    {
        // 1. 读取上次保存的 IP 地址（第一次运行默认是 192.168.1.xxx 方便修改）
        string savedIP = PlayerPrefs.GetString("LastROSIP", "192.168.");
        ipInputField.text = savedIP;

        // 2. 给按钮绑定点击事件
        connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    private void OnConnectButtonClicked()
    {
        string targetIP = ipInputField.text.Trim();

        // 检查输入是否为空
        if (string.IsNullOrEmpty(targetIP))
        {
            Debug.LogWarning("IP Address cannot be empty!");
            return;
        }

        // 保存这次输入的 IP，下次打开应用直接读取
        PlayerPrefs.SetString("LastROSIP", targetIP);
        PlayerPrefs.Save();

        // 两个连接共用同一个机器人 IP，但各自保留自己在 Inspector 中配置的端口
        Reconnect(imageConnection, targetIP);
        Reconnect(controlConnection, targetIP);
    }

    private void Reconnect(ROSConnection ros, string targetIP)
    {
        if (ros == null)
        {
            Debug.LogWarning("DynamicROSConnection: a ROSConnection reference is not assigned in the Inspector.");
            return;
        }

        // 断开现有的连接（防止它还在尝试连接之前的错误 IP）
        ros.Disconnect();

        Debug.Log($"Attempting to connect {ros.name} to ROS at {targetIP}:{ros.RosPort}");

        // 使用新 IP、原有端口重新发起连接
        ros.Connect(targetIP, ros.RosPort);
    }
}