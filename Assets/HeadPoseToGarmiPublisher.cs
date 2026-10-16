using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class HeadPoseToGarmiPublisher : MonoBehaviour
{
    [Tooltip("Drag the ROSConnection GameObject for the control endpoint (e.g. port 10001) here. Falls back to the default singleton if left empty.")]
    public ROSConnection ros;
    // public string topicName = "garmi_head/head_command";
    public string topicName = "/garmi/head/command";
    public float publishFrequency = 0.01f; // 100Hz 发送频率
    private float timeElapsed;

    // 根据你的机器人 URDF 实际情况，可能需要反转运动方向
    public bool invertTheta1 = true;
    public bool invertTheta2 = false;

    void Start()
    {
        // 获取 ROS 连接实例（若未在 Inspector 中指定，则退回默认单例）
        if (ros == null)
            ros = ROSConnection.GetOrCreateInstance();
        // 注册 Publisher
        ros.RegisterPublisher<Float64MultiArrayMsg>(topicName);
    }

    void Update()
    {
        // --- 新加的这行安全锁 ---
        // 检查是否已经建立了连接线程，如果没有，就直接 return 跳出，不执行后面的发送代码
        if (ros.HasConnectionThread == false) 
        {
            return; 
        }
        // ----------------------

        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishFrequency)
        {
            PublishHeadCommand();
            timeElapsed = 0;
        }
    }

    private void PublishHeadCommand()
    {
        // 1. 获取头显当前的欧拉角
        Vector3 eulerAngles = transform.localEulerAngles;

        // 2. 角度归一化处理 (Unity 的 eulerAngles 是 0~360，机器人需要 -180~180)
        float yawDeg = NormalizeAngle(eulerAngles.y);   // 左右
        float pitchDeg = NormalizeAngle(eulerAngles.x); // 上下

        // 3. 转换为弧度，并映射给 theta1 和 theta2
        double theta1 = yawDeg * Mathf.Deg2Rad * 0.5;
        double theta2 = pitchDeg * Mathf.Deg2Rad * 0.5;

        // 根据机器人坐标系方向调整符号
        if (invertTheta1) theta1 = -theta1;
        if (invertTheta2) theta2 = -theta2;

        // 4. 构造 Float64MultiArray 消息
        Float64MultiArrayMsg commandMsg = new Float64MultiArrayMsg();
        
        // 注意：标准 MultiArray 需要配置 layout，但对于简单的一维数组，通常只需填充 data 即可
        // 初始化 layout 以防 ROS 端报错 (取决于机器人的 Controller 实现)
        commandMsg.layout = new MultiArrayLayoutMsg();
        commandMsg.layout.dim = new MultiArrayDimensionMsg[1];
        commandMsg.layout.dim[0] = new MultiArrayDimensionMsg("joints", 2, 2);
        
        // 填入关节数据
        commandMsg.data = new double[] { theta1, theta2 };

        // 5. 发布消息
        ros.Publish(topicName, commandMsg);
    }

    /// <summary>
    /// 将 Unity 的 0~360 度角转换为 -180~180 度角
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f)
        {
            angle -= 360f;
        }
        else if (angle < -180f)
        {
            angle += 360f;
        }
        return angle;
    }
}