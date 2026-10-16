# Notes

当前已修改文件：

- `garmi_bringup/launch/sim/sim_garmi.launch.py`
- `garmi_bringup/config/sim/sim_garmi.yaml`
- `ROS-TCP-Endpoint`（git clone 的 ROS TCP 包）
- `garmi.xml`
- `zed_mini.obj`

---

## 19.04.26

实现了用 Unity 仿真中的头显运动控制 Docker 中 MuJoCo 仿真里 GARMI 机器人的头部运动。步骤如下：

### 1. 创建 Unity 项目并配置 ROS

导入 ROS-TCP-Connector 插件，点击菜单栏 **Robotics → ROS Settings**：

- **ROS IP Address**：`127.0.0.1`（Unity 编辑器在 Ubuntu 物理机上运行测试，ROS 2 运行在使用了 `--net=host` 的 Docker 中，所以填本机地址即可）
- **ROS Port**：`10000`

打开 **Edit → Project Settings → XR Plug-in Management → XR Interaction Toolkit**，勾选 **Use XR Device Simulator in scenes**。这样无须连接真实头显，可以直接用仿真头显测试。

### 2. 编写头显姿态发布脚本

写一个读取头显姿态并 publish 到目标 ROS 话题的脚本（`HeadPoseToGarmiPublisher.cs`），并将其拖动挂载到 VR Main Camera 上。

### 3. 在机器人 ROS 2 工作空间中添加 ROS-TCP-Endpoint

在 `multipanda_ros2/src` 目录下添加 Unity 官方的 ROS-TCP-Endpoint 包：

```bash
git clone -b main-ros2 https://github.com/Unity-Technologies/ROS-TCP-Endpoint.git
rosdep update
colcon build --packages-select ros_tcp_endpoint
```

### 4. 运行

**0）启动 Docker 容器**

```bash
docker start multipanda-container
docker exec -it --user developer multipanda-container bash
```

**1）在 Docker 中运行机器人 MuJoCo 仿真**

```bash
source ~/multipanda_ws/install/setup.bash
ros2 launch garmi_bringup sim_garmi.launch.py use_rviz:=true
```

**2）在 Unity 中点击播放按钮**，开始运行 VR 头显仿真，输入机器人仿真所在的 IP 地址。

**3）在 Docker 中运行 TCP 服务器中转站（ros_tcp_endpoint）**

> **20.04.26 更新**：此步骤已不再需要，ros_tcp_endpoint 已整合到机器人 MuJoCo 仿真（`sim_garmi.launch.py`）中。

```bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=0.0.0.0 -p ROS_TCP_PORT:=10000
```

查看头部指令话题：

```bash
ros2 topic echo /garmi_head/head_command
```

---

## 20.04.26

在 Unity 仿真中新增了用 VR 控制器输入 ROS IP 的 UI。将 Unity 仿真打包为 APK 并安装到 Quest 3 头显上，经过通讯调试，运行成功。

Unity 中 build 出 APK 后，用 USB 线连接头显和 Ubuntu 电脑，在头显中选择**允许 USB 调试**，然后在 Ubuntu 中 APK 所在目录打开终端，用 adb 命令把 APK 传到头显中（`/data/local/tmp/`）并安装。

### USB 连接安装 APK 到头显

**1）确认头显已连接**（应显示设备序列号，状态为 `device`；若显示 `unauthorized`，请在头显中允许 USB 调试）

```bash
adb devices
```

**2）查看头显中存储的现有（旧版）安装包**

```bash
adb shell ls -l /data/local/tmp/
```

**3）删除旧版安装包**

```bash
adb shell rm /data/local/tmp/Quest3-ROS2-garmi-head.apk
```

（可选）卸载头显中已安装的旧版应用（包名见 Project Settings → Player → Package Name，当前为 `com.DefaultCompany.VRTemplate`）：

```bash
adb uninstall com.DefaultCompany.VRTemplate
```

**4）推送新版安装包到头显，然后安装**

```bash
adb push Quest3-ROS2-garmi-head.apk /data/local/tmp/
adb shell pm install -r /data/local/tmp/Quest3-ROS2-garmi-head.apk
```
