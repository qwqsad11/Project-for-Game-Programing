# 《石山羊》项目执行卡片

## 🚀 快速启动

### 第1天（Day 1）：项目初始化
```
任务清单:
□ 创建Unity项目（2D模板）
□ 导入必要包（TextMesh Pro）
□ 创建文件夹结构
□ 配置iOS/Android设置
□ 创建3个基础场景

时间估计: 2-3小时
```

### 第1周完成目标
✅ 项目结构完整
✅ GameManager + 状态机运行
✅ 山羊移动系统工作
✅ 基础场景转换

---

## 📊 关键数据

| 指标 | 目标值 |
|-----|------|
| 总开发周期 | 4-6周 |
| 总任务数 | 40个Issue |
| 第一个里程碑 | 2周完成核心玩法 |
| 测试周期 | 1周 |
| 发布准备 | 3-5天 |

---

## 🎯 核心机制实现优先级

### 必做（Week 1-2）
1. ✅ 山羊移动（左右跳跃）
2. ✅ 棋盘式无限关卡生成
3. ✅ 5种障碍物
4. ✅ 饥饿和草地系统
5. ✅ 基础碰撞检测

### 重要（Week 2-3）
6. ✅ 金币系统
7. ✅ 道具系统（弹跳板、无敌）
8. ✅ 角色皮肤切换
9. ✅ 主题系统

### 增强（Week 3-4）
10. ✅ 音效系统
11. ✅ UI完整
12. ✅ 数据存储

### 可选（Week 4-6）
13. ⭕ 内购系统
14. ⭕ 广告集成
15. ⭕ 排行榜

---

## 📁 推荐代码文件结构

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          🔴
│   │   ├── AudioManager.cs
│   │   └── SaveManager.cs
│   │
│   ├── Gameplay/
│   │   ├── GoatController.cs       🔴
│   │   ├── HungerSystem.cs         🔴
│   │   ├── LevelGenerator.cs       🔴
│   │   ├── Obstacle.cs             🔴
│   │   ├── Platform.cs
│   │   ├── PowerUp.cs
│   │   └── CoinSystem.cs
│   │
│   ├── UI/
│   │   ├── MainMenuUI.cs
│   │   ├── HudUI.cs
│   │   ├── GameOverUI.cs
│   │   └── SkinSelectionUI.cs
│   │
│   ├── Theme/
│   │   ├── ThemeSystem.cs
│   │   └── Theme.cs
│   │
│   └── Data/
│       ├── PlayerSaveData.cs
│       └── GoatSkinData.cs
│
├── Prefabs/
│   ├── Platform.prefab
│   ├── Obstacle_Log.prefab
│   ├── Obstacle_Rock.prefab
│   ├── Coin.prefab
│   ├── Grass.prefab
│   └── GoatUI.prefab
│
├── Scenes/
│   ├── MainMenu.unity              🔴
│   ├── GamePlay.unity              🔴
│   └── GameOver.unity
│
├── Art/
│   ├── Models/
│   │   ├── Goat.fbx
│   │   └── Obstacles/
│   ├── Textures/
│   │   ├── Platform_*.png
│   │   └── Theme_*/
│   └── Animations/
│       └── Goat.controller
│
└── Audio/
    ├── Music/
    │   └── background.mp3
    └── SFX/
        ├── jump.wav
        ├── land.wav
        ├── eat.wav
        ├── coin.wav
        └── death.wav

注: 🔴 = 第1周必做
```

---

## 💡 技术架构建议

### 架构模式：MVC + 单例

```
View（UI层）
   ↓
Controller（输入处理）
   ↓
Model（数据管理）
   ↓
Manager（系统管理）

单例管理器：
- GameManager (全局)
- AudioManager (全局)
- SaveManager (全局)
- UIManager (全局)
```

### 事件系统

```csharp
// 推荐使用事件而不是直接调用
public class GameManager
{
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<GameState> OnStateChanged;
    public static event System.Action OnGameOver;
}

// 订阅者
private void OnEnable()
{
    GameManager.OnGameOver += HandleGameOver;
}
```

---

## 🎨 美术外包清单

如果需要外包美术：

### 必需资源
- [ ] 山羊3D模型（基础+2种变体）
- [ ] 障碍物模型（圆木、落石）
- [ ] 平台和地形贴图
- [ ] 天空盒（3个主题）
- [ ] UI图标和背景
- [ ] 粒子特效（5种）

### 预算参考
- 基础3D模型: $50-200
- 纹理和贴图: $30-100
- UI设计: $50-100
- 特效: $50-100
- **总计**: $200-500

### 推荐外包平台
- Fiverr / Upwork (个人外包)
- CGTrader (模型库)
- Unity Asset Store (购买现成)

---

## 🔊 音效外包清单

### 必需音效
- [ ] 背景音乐 (30秒循环)
- [ ] 5个主要SFX (跳跃、着陆、进食、金币、死亡)
- [ ] 3个UI音效

### 预算参考
- 背景音乐: $20-100
- 单个SFX: $5-20
- **总计**: $50-200

### 推荐来源
- Fiverr (作曲家)
- FreeSound.org (免费)
- Unity Asset Store

---

## 📱 性能目标

### 移动设备目标
```
iPhone:
- 最低支持: iPhone 8 (2018)
- 目标帧率: 60 FPS
- 内存占用: < 150MB
- 启动时间: < 3秒

Android:
- 最低支持: Android 9 (API 28)
- 目标帧率: 60 FPS
- 内存占用: < 200MB
- 启动时间: < 3秒
```

### 优化检查表
```
□ 使用Object Pool管理对象
□ 启用批处理
□ 压缩纹理（ASTC/ETC2）
□ 限制粒子数量（< 100）
□ 使用LOD减少模型面数
□ 异步加载资源
□ 定期检查内存泄漏
```

---

## 🧪 测试清单

### 功能测试
```
□ 移动系统: 左右移动、碰撞、着陆
□ 障碍物: 5种全部生成、伤害检测、特效
□ 饥饿系统: 递减、恢复、饿死
□ 金币: 拾取、计数、保存
□ 皮肤: 切换、主题应用、保存
□ UI: 按钮、过渡、数据显示
```

### 兼容性测试
```
□ iOS: iPhone SE, 12, 12 Pro Max
□ Android: Pixel 5, 小米, 华为
□ 屏幕: 16:9, 18:9, 19.5:9, 平板
□ 系统: iOS 13+, Android 9+
```

### 性能测试
```
□ 帧率: 平均60 FPS, 最低45 FPS
□ 内存: < 150MB (移动)
□ 电池: 1小时游戏消耗< 15%
□ 网络: 无网络也可游玩
```

---

## 📊 里程碑检查点

### Milestone 1: MVP（最小可玩版）- Week 2
- [ ] 山羊能移动
- [ ] 关卡能生成
- [ ] 能死亡和重启
- [ ] 保存最高分

**评价**: 可玩，但不完整

### Milestone 2: 功能完整 - Week 3
- [ ] 所有游戏机制完成
- [ ] UI基本完整
- [ ] 3个皮肤+ 3个主题
- [ ] 音效集成

**评价**: 内容丰富，可测试

### Milestone 3: 优化完成 - Week 4
- [ ] 性能稳定 (60 FPS)
- [ ] 没有关键bug
- [ ] UI响应式布局
- [ ] 内购/广告集成

**评价**: 发布就绪

### Milestone 4: 发布 - Week 5
- [ ] 通过App Store审核
- [ ] 通过Google Play审核
- [ ] 上线可用

**评价**: 成功发布！

---

## 🐛 常见坑洞

### 可能遇到的问题

| 问题 | 症状 | 解决方案 |
|-----|------|--------|
| 山羊卡进障碍物 | 无法移动 | 调整碰撞体，添加间隙 |
| 内存泄漏 | 长时间游玩卡顿 | 使用Object Pool |
| UI超出屏幕 | 部分UI被切割 | 使用SafeArea，测试多分辨率 |
| 音效重复播放 | 声音混乱 | 使用音效池管理 |
| 存档丢失 | 重启后数据消失 | 检查存储路径和序列化 |
| 难度过高 | 玩家立即死亡 | 调整障碍物密度，增加飞行时间 |
| FPS波动 | 帧率不稳定 | Profile查找峰值，优化GC |

---

## 📈 发布后运营

### 第1个月
- 监控下载量和崩溃率
- 收集玩家反馈
- 修复bug和性能问题
- 发布 v1.0.1 补丁

### 第2-3个月
- 添加新皮肤 (4-8个)
- 优化难度曲线
- 添加每日任务
- 发布 v1.1 功能更新

### 第3个月以后
- 完整33种皮肤
- 季节活动
- 排行榜系统
- 持续优化和维护

---

## 🤝 团队协作建议

### 工作流
1. **Scrum冲刺**: 每周一个冲刺
2. **Daily Standup**: 每天同步进度
3. **Code Review**: Pull Request前审查
4. **Release Checklist**: 发布前完整检查

### Git分支策略
```
main (发布分支)
  ↑
develop (开发分支)
  ↑
feature/* (功能分支)
```

### 建议Git Commit消息格式
```
[type]: brief description

type: feat|fix|refactor|docs|style|perf|test|chore

示例:
feat: implement goat movement system
fix: resolve collision detection bug
refactor: optimize level generation
```

---

## 🎓 参考资源

### Unity学习
- [Unity Learn Platform](https://learn.unity.com/)
- [Brackeys YouTube频道](https://www.youtube.com/c/Brackeys)
- [Unity官方教程](https://docs.unity3d.com/)

### 游戏设计
- [Game Design Document模板](https://www.gamasutra.com/)
- [Difficulty Curve设计](https://www.gamesindustry.biz/)

### 代码示例
- [GitHub: Flappy Bird Clone](https://github.com/)
- [GitHub: 2D Platformer](https://github.com/)

### 美术资源
- [Kenney.nl - 免费游戏资源](https://kenney.nl/)
- [Sketchfab - 3D模型](https://sketchfab.com/)
- [OpenGameArt - 开源资源](https://opengameart.org/)

### 音效资源
- [FreeSound.org](https://freesound.org/)
- [Zapsplat - 免费音效](https://www.zapsplat.com/)

---

## ✅ 发布前最终清单

```
代码质量
□ 没有编译错误
□ 没有关键警告
□ 代码风格一致
□ 注释和文档完整

性能
□ 60 FPS稳定性 > 95%
□ 内存占用 < 150MB
□ 启动时间 < 3秒
□ 没有内存泄漏

功能
□ 所有游戏机制工作
□ 所有UI元素显示正确
□ 音效正常播放
□ 存档系统正常

兼容性
□ iOS 13+ 通过
□ Android 9+ 通过
□ 多分辨率测试通过
□ 真机测试通过

内容
□ 图标和截图准备
□ 应用描述编写
□ 隐私政策完成
□ 本地化检查（如需）

商务
□ 内购产品配置
□ 广告账户配置
□ 分析系统集成
□ 统计跟踪完成
```

---

## 💬 快速提问答案

**Q: 多久能做出来?**
A: 4-6周（全职开发者1人）

**Q: 需要多少人?**
A: 最小1人（全栈）+ 1人美术

**Q: 美术是自己做还是外包?**
A: 推荐混合，基础外包，主题自制

**Q: 会赚钱吗?**
A: 不确定，但可通过内购+广告变现

**Q: 开源吗?**
A: 看你，建议不完全开源但分享经验

**Q: 碰到bug怎么办?**
A: 查看常见坑洞，Google搜索，Unity论坛

---

**项目创建时间**: 2026年4月
**最后更新**: 2026年4月24日
**版本**: 1.0

