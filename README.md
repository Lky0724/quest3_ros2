# Quest3-ROS2

Unity project for Meta Quest 3, built on the Unity XR VR template, that integrates with ROS2 to stream stereo video from the headset and publish head-pose data to a robot (Garmi).

## Requirements

- Unity Hub
- Unity Editor **6000.1.7f1** (Unity 6) — Unity Hub will prompt to install the matching version automatically when you open the project
- Android Build Support module (with OpenJDK, Android SDK & NDK Tools) for building the Quest 3 APK

## Key scripts

- `Assets/DynamicROSConnection.cs` — manages the ROS2 connection
- `Assets/HeadPoseToGarmiPublisher.cs` — publishes headset pose to the Garmi robot
- `Assets/ROSStereoSubscriber.cs` — subscribes to and renders the stereo camera feed from ROS2
- `Assets/VRNumpad.cs` — in-VR numeric input

## Getting started

1. Clone the repo:
   ```bash
   git clone https://github.com/Lky0724/quest3_ros2.git
   ```
2. Open Unity Hub → **Add** → select the cloned folder. Unity Hub will offer to install the required editor version if it's missing.
3. Open the project. Unity will regenerate `Library/`, `Temp/`, and the `.csproj`/`.sln` files on first load — this can take a few minutes.

## Building the APK

1. `File → Build Settings`, select **Android** as the platform (switch platform if needed).
2. Make sure a Quest 3 device (or the target Android build target) is connected/selected.
3. `Build` (or `Build And Run` with the headset connected via USB/ADB) to produce the `.apk`.

Built `.apk` files are not tracked in this repo (see `.gitignore`) — distribute builds via GitHub Releases or another artifact channel.
