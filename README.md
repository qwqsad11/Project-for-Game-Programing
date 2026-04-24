# 太阳系项目 (Solar System Project)

这是一个使用 Unity 引擎开发的太阳系交互式可视化项目。

## 项目简介

该项目展示了太阳系中各个天体的运动和交互。用户可以通过点击或交互来观察不同行星的信息。

## 项目结构

```
Assets/
├── Scenes/                 # 场景文件
│   └── SampleScene.unity   # 主场景
├── SolarSystemAssets2026/  # 太阳系资源
│   ├── Scripts/            # C# 脚本文件
│   ├── Materials/          # 材质文件
│   ├── Textures/           # 纹理文件
│   ├── Icons/              # 图标资源
│   └── Sounds/             # 音效文件
└── TextMesh Pro/           # TextMesh Pro 文本组件资源
```

## 核心脚本

- **CameraFocusController.cs** - 摄像机焦点控制
- **CelestialBody.cs** - 天体基类
- **RotateAround.cs** - 围绕中心天体旋转
- **SolarSystemInteractor.cs** - 太阳系交互系统
- **SolarSystemUIManager.cs** - UI 管理器
- **Spawner.cs** - 对象生成器
- **LookAtTarget.cs** - 看向目标的脚本
- **Projectile.cs** - 抛物体脚本

## 使用的技术

- Unity 3D 引擎
- C# 编程语言
- TextMesh Pro 文本组件
- 3D 图形渲染

## 安装和运行

1. 确保安装了 Unity 2022 或更新版本
2. 克隆此项目到本地
3. 用 Unity Hub 打开项目
4. 在 Unity 编辑器中打开 SampleScene
5. 点击播放按钮运行项目

## 许可证

此项目仅供学习和个人使用。

---

**创建时间**: 2026年4月
