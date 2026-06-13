# GitHub Issues导入模板

## 使用说明
将下面的Issue内容复制到GitHub项目中。GitHub支持批量创建，建议：
1. 在GitHub项目的"Issues"页面
2. 逐个创建或使用GitHub CLI批量创建
3. 使用Labels标签分类（如core-system, gameplay, art等）
4. 在Projects中创建看板关联这些Issue

---

## Phase 1：基础架构

### Issue #1
**标题**: 项目初始化与文件夹结构
**标签**: `setup`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
初始化Unity项目和项目文件夹结构

## 子任务
- [ ] 创建Unity 2020+ 2D项目
- [ ] 导入TextMesh Pro和输入系统包
- [ ] 创建标准文件夹结构
  - Scripts/
    - Core/
    - Gameplay/
    - UI/
    - Audio/
    - Data/
  - Prefabs/
  - Scenes/
  - Art/
    - Models/
    - Textures/
    - Animations/
  - Audio/
    - Music/
    - SFX/
  - Resources/
- [ ] 配置iOS/Android平台设置
- [ ] 设置启动场景

## 验收条件
- 项目可以正常打开和构建
- 所有文件夹结构清晰
- 项目设置配置完成
```

---

### Issue #2
**标题**: GameManager单例与游戏状态机
**标签**: `core-system`, `architecture`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现全局GameManager管理器，负责游戏全局状态和流程

## 功能需求
- 单例模式实现（不在场景切换时销毁）
- 游戏状态枚举: MenuState, PlayingState, GameOverState, PausedState
- 状态切换回调系统
- 分数和高分管理
- 游戏暂停/恢复逻辑

## 代码示例
```csharp
public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, Paused }
    public static GameManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }
    public event System.Action<GameState> OnStateChanged;
    
    // 分数管理
    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }
}
```

## 验收条件
- 状态机正常切换
- 分数存取功能正常
- 暂停/恢复功能完整
```

---

### Issue #3
**标题**: 主菜单、游戏、结果场景结构
**标签**: `setup`, `scene`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
创建游戏基础场景结构

## 场景列表
1. MainMenu场景 - 主菜单和角色选择
2. GameScene场景 - 主游戏场景
3. GameOver场景 - 结果和统计

## 每个场景的基本结构
- Canvas (UI Root)
- MainCamera
- GameManager Reference
- AudioManager Reference
- EventSystem

## 验收条件
- 三个场景都能正常打开和转换
- 各场景基本结构完整
- 场景间转换不出现错误
```

---

## Phase 2：玩家角色系统

### Issue #4
**标题**: 山羊移动控制与输入系统
**标签**: `gameplay`, `input`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现山羊左右跳跃的核心控制系统

## 功能需求
- 检测屏幕点击位置（左/右半边）
- 实现网格式跳跃（每次移动一格）
- 山羊只能跳向高一层的平台
- 实现回头拖拽返回机制

## 输入映射
- 点击屏幕左半部分 → 山羊向左跳
- 点击屏幕右半部分 → 山羊向右跳
- 长按屏幕拖拽 → 山羊可回头（斜向下）

## 代码框架
```csharp
public class GoatController : MonoBehaviour
{
    private Vector2Int gridPosition; // 网格位置 (X, Z)
    
    private void OnScreenTouched(Vector2 touchPosition)
    {
        if (touchPosition.x < Screen.width * 0.5f)
            MoveLeft();
        else
            MoveRight();
    }
    
    public void MoveLeft() { /* ... */ }
    public void MoveRight() { /* ... */ }
}
```

## 验收条件
- 山羊能正确响应左右点击
- 移动动画流畅
- 碰撞检测准确
```

---

### Issue #5
**标题**: 山羊模型导入与动画系统
**标签**: `art`, `animation`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
导入山羊3D模型并设置基础动画

## 需要的资源
- 山羊基础模型 (支持多种形状变形)
- Animator Controller配置

## 动画集合
- Idle: 待机动画
- Walk: 行走动画
- Jump: 跳跃动画
- Die: 死亡动画
- Eat: 进食动画
- Happy: 获得道具时的动画

## 物理设置
- Rigidbody: 启用重力
- Collider: BoxCollider用于碰撞检测
- 图层设置: "Player"

## 验收条件
- 模型在场景中正确显示
- 所有动画能正确播放
- 动画切换流畅无卡顿
```

---

### Issue #6
**标题**: 山羊皮肤与角色解锁系统
**标签**: `gameplay`, `ui`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现多角色皮肤系统，支持33种不同山羊

## 数据结构
```csharp
[System.Serializable]
public class GoatSkin
{
    public int id;
    public string name;
    public string description;
    public Mesh model;
    public Material[] materials;
    public Theme theme; // 关联主题
    public bool isUnlocked;
}
```

## 功能需求
- 创建GoatSkinManager管理类
- 实现皮肤切换逻辑
- UI选择菜单（显示已解锁和锁定皮肤）
- 本地存储已解锁皮肤列表
- 显示解锁条件提示

## 初期计划
首发包含5-8个皮肤，后续逐步添加

## 验收条件
- 能正确切换已解锁皮肤
- 新解锁皮肤显示解锁提示
- 皮肤数据能正确保存和读取
```

---

## Phase 3：山体与关卡生成

### Issue #7
**标题**: 棋盘式无限关卡生成器
**标签**: `gameplay`, `procedural`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现可无限向上生成的棋盘式山体

## 技术方案
- 坐标系: X轴(左右), Z轴(深度)
- 每个平台是一个立方体格子
- 使用对象池(Object Pool)优化性能
- 离视场远的平台自动销毁

## 算法需求
- 动态生成前方和上方平台
- 生成概率平衡（确保可通过但有难度）
- 平台类型随机分布（安全、草地、障碍物等）

## 关键数学
```csharp
public class LevelGenerator : MonoBehaviour
{
    private const int GRID_SIZE = 1f; // 网格大小
    private Queue<Platform> platformPool;
    
    public void GeneratePlatforms(Vector3 playerPos) { /* ... */ }
}
```

## 性能目标
- 保持活跃平台< 200个
- 内存占用< 100MB
- 生成延迟< 1帧

## 验收条件
- 平台能无限生成
- 不会出现无法通过的死路
- 性能稳定
```

---

### Issue #8
**标题**: 5种基础障碍物实现
**标签**: `gameplay`, `obstacles`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现游戏中的5种主要障碍物

### 1. 圆木 (RollingLog)
- 沿固定方向直线滚动
- 可预测运动轨迹
- 造成接触伤害

### 2. 落石 (FallingRock)
- 垂直下落
- 每0.3-0.5秒随机改变方向
- 高难度威胁

### 3. 雷云 (LightningCloud)
- 固定位置悬浮
- 每2-3秒释放闪电波
- 只能躲避或绕道

### 4. 碎石路 (CrumblingPlatform)
- 踩踏后触发计时器
- 0.5秒后崩塌消失
- 需要快速跳离

### 5. 断崖/河流 (Gap)
- 不可跨越的空地
- 必须利用弹跳板或其他工具

## 实现框架
```csharp
public abstract class Obstacle : MonoBehaviour
{
    public virtual void OnPlayerCollide(GoatController goat)
    {
        goat.Die();
    }
}
```

## 验收条件
- 所有障碍物都能正确生成和运动
- 碰撞检测准确
- 视觉反馈清晰
```

---

### Issue #9
**标题**: 碰撞检测与死亡系统
**标签**: `gameplay`, `physics`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现完整的物理碰撞和死亡机制

## 物理层配置
- Player: 玩家层
- Platform: 平台层
- Obstacle: 障碍物层
- Trigger: 触发器（食物、金币）

## 碰撞规则
- Player与Obstacle碰撞 → 死亡
- Player与Platform碰撞 → 正常着陆
- Player与Trigger碰撞 → 拾取效果

## 死亡反馈
- 播放死亡动画
- 显示墓碑特效
- 播放死亡音效
- 2秒后显示结果界面

## 代码框架
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Obstacle"))
        Die();
    else if (other.CompareTag("Food"))
        EatFood();
}

public void Die()
{
    animator.SetTrigger("Die");
    gameManager.GameOver();
}
```

## 验收条件
- 碰撞检测准确无误
- 死亡流程完整
- 无穿模现象
```

---

## Phase 4：关键游戏机制

### Issue #10
**标题**: 饥饿系统实现
**标签**: `gameplay`, `mechanic`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现山羊饱饿度管理系统

## 机制设计
- 饥饿值范围: 0-100
- 初始饥饿值: 100（满）
- 每秒掉饥饿值: 5点/秒（可调）
- 饥饿警告: 饥饿值< 30时，画面开始变暗/发红

## 饿死判定
- 饥饿值≤0 → 山羊饿死
- 触发死亡特效和游戏结束

## 代码结构
```csharp
public class HungerSystem : MonoBehaviour
{
    private float hungerValue = 100f;
    private const float HUNGER_DECAY_RATE = 5f; // 每秒
    
    public void Eat(float amount)
    {
        hungerValue = Mathf.Min(100f, hungerValue + amount);
    }
    
    public bool IsDead => hungerValue <= 0;
}
```

## 游戏平衡
- 草地恢复: 30点
- 关卡难度需要权衡，确保不会因饥饿强制死亡

## 验收条件
- 饥饿值正常递减
- 饿死机制正确触发
- 饥饿警告UI正常显示
```

---

### Issue #11
**标题**: 草地拾取系统
**标签**: `gameplay`, `mechanic`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
实现食物拾取和饥饿恢复机制

## 功能需求
- 创建Grass预制体（特殊平台）
- 拾取触发器检测
- 进食动画和音效
- 饥饿值恢复逻辑

## Grass设计
- 视觉上与普通平台区分（更绿/有草纹理）
- 拾取后消失
- 可通过关卡生成器配置生成密度

## 恢复值
- 单次进食恢复: 30点饥饿值
- 多个Grass可连续拾取

## 验收条件
- 山羊能正确拾取草地
- 饥饿值正确恢复
- 拾取反馈清晰（动画+音效）
```

---

### Issue #12
**标题**: 金币系统实现
**标签**: `gameplay`, `economy`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
实现游戏中的货币系统

## 功能需求
- 创建Coin预制体
- 拾取逻辑
- 金币计数UI显示
- 本地存储总金币数

## 金币掉落来源
- 地面拾取: 1-5枚随机
- 本局完成奖励: 高度* 0.1
- 特殊成就: 完成特殊条件额外奖励
- 广告观看: 视频后+50枚

## 数据持久化
```csharp
public class CurrencyManager
{
    public int TotalCoins { get; set; }
    public int CurrentSessionCoins { get; set; } // 本局获得
}
```

## 验收条件
- 金币能正确拾取和计数
- 金币数据能正确存储
- UI显示准确
```

---

### Issue #13
**标题**: 道具系统实现
**标签**: `gameplay`, `power-up`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
实现两种增强道具机制

## 道具1: 弹跳板 (Bouncer)
- 踩踏触发弹射效果
- 将山羊弹射5-10格远距离
- 视觉反馈: 弹簧动画
- 可能落在碎石路上，需要快速反应

## 道具2: 弹簧 (Shield)
- 获得后进入5秒无敌状态
- 无敌期间可无视所有伤害
- 视觉变化: 山羊变为"钢铁羊"（颜色变化/发光）
- 无敌状态结束时播放还原动画

## 通用设计
```csharp
public abstract class PowerUp : MonoBehaviour
{
    public virtual void Activate(GoatController goat) { }
}
```

## 掉落条件
- 随机生成在特定平台上
- 出现概率: 2-5%
- 显示发光特效吸引玩家

## 验收条件
- 道具能正确激活
- 效果时间准确
- 视觉反馈清晰
```

---

## Phase 5：主题系统与美术

### Issue #14
**标题**: 动态场景主题系统
**标签**: `art`, `theme`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
实现根据角色皮肤动态改变场景风格的主题系统

## 主题包含元素
1. 背景天空
2. 平台颜色/纹理
3. 光源和阴影
4. 环境特效（雾、粒子等）
5. 障碍物样式
6. UI颜色主题

## 数据结构
```csharp
[System.Serializable]
public class Theme
{
    public string name;
    public Color platformColor;
    public Texture2D platformTexture;
    public Material skyboxMaterial;
    public Color lightColor;
    public ParticleSystem[] environmentParticles;
}
```

## 主题应用流程
1. 玩家选择角色皮肤
2. 获取皮肤关联的主题
3. 应用主题到所有对象
4. 平滑过渡动画（可选）

## 验收条件
- 主题能正确应用
- 场景元素全部响应主题变化
- 过渡流畅
```

---

### Issue #15
**标题**: 初期主题内容制作
**标签**: `art`, `content`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
制作5-10个初期发布主题

## 主题列表（优先级顺序）
1. 原始主题 - 默认风格（绿草+蓝天）
2. 樱花和风主题 - 樱花飘落，灯笼，日本建筑
3. 夜视镜主题 - 绿色荧光，红外效果
4. 像素复古主题 - 8bit风格，复古音效
5. 地下洞穴主题 - 石头纹理，发光蘑菇
6. 冰雪主题 - 冰蓝色，雪花特效
7. 沙漠主题 - 黄褐色，尘暴特效
8. 太空主题 - 深紫色，星星和UFO

## 每个主题需要
- 天空盒(Skybox)
- 颜色和纹理配置
- 环境粒子特效
- 独特的视觉标识

## 美术资源来源
- Unity Asset Store
- Kenney.nl(免费资源)
- 自主绘制

## 验收条件
- 每个主题有明显的视觉区别
- 所有主题运行流畅(性能> 30FPS)
```

---

### Issue #16
**标题**: 特效系统实现
**标签**: `art`, `vfx`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
添加各种视觉反馈特效

## 特效清单
1. 草地吃掉特效
   - 绿色粒子喷出
   - 音效反馈

2. 金币收集特效
   - 金币缩小并飞向UI计数器
   - 收集音效

3. 道具激活特效
   - 弹簧弹射闪光
   - 无敌盾牌发光

4. 死亡/墓碑特效
   - 山羊消失
   - 墓碑出现动画
   - 悲伤音效

5. 伤害闪烁
   - 碰撞到障碍物时，山羊短暂闪烁红色

## 实现工具
- Unity内置粒子系统
- Shader特殊效果
- Animator动画

## 验收条件
- 所有特效都能正确触发
- 特效性能占用< 2ms
- 特效与游戏事件同步
```

---

## Phase 6：音频系统

### Issue #17
**标题**: 音效管理系统
**标签**: `audio`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现完整的音效管理和播放系统

## 音效需求
1. 背景音乐
   - 主菜单音乐（循环）
   - 游戏进行中背景音乐（循环）
   - 根据高度逐渐变化（可选）

2. 游戏SFX
   - 跳跃音效
   - 着陆音效
   - 进食草地音效
   - 拾取金币音效
   - 碰撞伤害音效
   - 死亡音效
   - 道具激活音效

3. UI音效
   - 按钮点击音效
   - 菜单转换音效
   - 解锁角色通知音效

## 音效管理器设计
```csharp
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; }
    
    public void PlaySFX(string clipName);
    public void PlayMusic(string clipName, bool loop = true);
    public void StopMusic();
}
```

## 音效资源来源
- FreeSound.org
- Zapsplat.com
- Unity Asset Store

## 验收条件
- 所有音效能正确播放
- 没有音量问题或失真
- 背景音乐无缝循环
```

---

### Issue #18
**标题**: 主题音效切换
**标签**: `audio`, `theme`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现根据主题改变音效风格

## 需求
- 每个主题可关联不同的音效库
- 切换角色时自动切换音效
- 保持音效音量平衡

## 主题音效变化示例
- 原始主题: 卡通音效
- 和风主题: 日本风笛音效
- 夜视镜主题: 科幻嗡嗡声
- 像素主题: 8bit复古音效

## 实现方式
```csharp
[System.Serializable]
public class ThemeAudioSet
{
    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;
    public AudioClip deathSound;
    // ...
}
```

## 验收条件
- 音效库能正确切换
- 没有音效丢失或错位
```

---

## Phase 7：UI与菜单系统

### Issue #19
**标题**: 主菜单UI实现
**标签**: `ui`, `menu`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
创建游戏主菜单界面

## UI元素
1. 游戏标题和Logo
2. 开始游戏按钮
3. 设置按钮
4. 最高分显示
5. 音量控制（可选）

## 布局建议
- 标题在顶部
- 大的开始按钮在中央
- 设置和统计在底部
- 响应式布局，支持不同分辨率

## 动画效果
- Logo淡入
- 按钮悬停高亮
- 场景转换动画

## 验收条件
- 所有按钮功能正常
- UI显示清晰
- 响应式布局有效
```

---

### Issue #20
**标题**: 山羊皮肤选择界面
**标签**: `ui`, `menu`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
创建角色皮肤选择菜单

## UI功能
1. 显示所有已解锁皮肤
2. 显示锁定的皮肤（灰显）
3. 解锁条件提示（需要X金币）
4. 角色3D预览
5. 主题效果预览
6. 选择/确认按钮

## 布局
- 左侧: 皮肤列表（可滚动）
- 右侧: 3D模型预览
- 底部: 解锁信息和开始按钮

## 交互
- 点击皮肤卡片预览
- 点击开始游戏使用选中皮肤
- 显示解锁倒计时或价格

## 验收条件
- 能正确显示所有皮肤
- 预览功能正常
- 3D模型旋转显示流畅
```

---

### Issue #21
**标题**: 游戏内HUD实现
**标签**: `ui`, `hud`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
创建游戏进行中的信息显示UI

## HUD元素
1. 高度/海拔显示
   - 实时显示当前爬升高度
   - 与分数关联

2. 饥饿度条
   - 绿色满，红色饥饿
   - 低于30%时警告
   - 可选数字显示

3. 金币计数
   - 当前获得金币数
   - 金币图标
   - 飞行动画

4. 暂停按钮
   - 右上角
   - 点击显示暂停菜单

5. FPS显示（开发用）
   - 右上角小字
   - 仅Debug模式显示

## 布局
- 高度和金币: 顶部
- 饥饿度条: 顶部或左侧
- 暂停按钮: 右上角
- 响应式布局

## 更新频率
- 高度: 每帧更新
- 金币: 拾取时更新
- 饥饿值: 每秒更新

## 验收条件
- 所有元素显示准确
- 更新频率正确
- 不影响游戏性能
```

---

### Issue #22
**标题**: 游戏结束结算界面
**标签**: `ui`, `menu`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
创建死亡后的结果显示界面

## 显示信息
1. 最终高度（分数）
2. 本局金币获得数
3. 新解锁皮肤通知（如有）
4. 与历史最高分对比
5. 广告按钮（可选奖励）

## 界面布局
- 标题: "Game Over"
- 大数字显示最终分数
- 统计信息列表
- 底部按钮
  - 重新开始
  - 返回菜单
  - 观看广告（+奖励）

## 动画效果
- 从底部滑入
- 分数滚动增长动画
- 按钮缩放反馈

## 验收条件
- 所有信息正确显示
- 按钮功能正常
- 动画流畅
```

---

### Issue #23
**标题**: 新角色解锁展示动画
**标签**: `ui`, `animation`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
创建获得新角色时的展示效果

## 触发条件
- 获得100金币开启山羊箱子
- 随机解锁一个新皮肤

## 展示流程
1. 停顿游戏（暂停）
2. 显示"获得新角色"弹窗
3. 3D模型缓慢旋转展示
4. 播放解锁音效
5. 显示角色名称和主题描述
6. 闪光/烟雾特效

## UI设计
- 中央大弹窗
- 3D模型展示
- 角色信息和主题预览
- 确认按钮

## 验收条件
- 解锁展示流程完整
- 3D预览流畅
- 特效和音效同步
```

---

## Phase 8：移动平台优化

### Issue #24
**标题**: 手机输入系统优化
**标签**: `input`, `mobile`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
优化触屏输入体验

## 优化项目
1. 屏幕点击响应时间
   - 目标: < 50ms

2. 屏幕边缘识别
   - 确保边缘点击不被误判
   - 处理刘海屏安全区域

3. 多分辨率兼容
   - 测试16:9, 18:9, 19.5:9等比例
   - 确保点击区域准确

4. 触摸反馈
   - 点击时播放按压反馈（若设备支持）
   - 视觉反馈（点击位置动画）

## 代码改进
```csharp
private Vector2 GetScreenClickPercentage(Touch touch)
{
    return touch.position / new Vector2(Screen.width, Screen.height);
}
```

## 测试设备
- iOS: iPhone 12/13/14系列
- Android: Pixel 5/6系列, 小米, 华为等

## 验收条件
- 输入延迟< 50ms
- 所有分辨率下点击准确
- 边缘识别正确
```

---

### Issue #25
**标题**: 性能优化与帧率稳定
**标签**: `optimization`, `performance`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
确保游戏在移动设备上流畅运行

## 优化清单
1. 对象池(Object Pool)
   - [ ] 平台对象池
   - [ ] 障碍物对象池
   - [ ] 粒子效果池

2. 渲染优化
   - [ ] 批处理设置
   - [ ] LOD设置（关闭暂时不用）
   - [ ] 动态批处理启用

3. 物理优化
   - [ ] 物理迭代次数调整
   - [ ] 固定时间步优化
   - [ ] 碰撞检测优化

4. 内存管理
   - [ ] 纹理压缩(ASTC或ETC2)
   - [ ] 模型LOD
   - [ ] 音频优化（压缩格式）

## 性能目标
- 平均帧率: 60 FPS
- 最低帧率: 45 FPS
- 内存占用: < 150MB
- GPU占用: < 80%

## 测试工具
- Unity Profiler
- Xcode Instruments (iOS)
- Android Profiler

## 验收条件
- 60 FPS稳定性> 95%
- 无明显卡顿
- 内存占用在预算内
```

---

### Issue #26
**标题**: 屏幕分辨率适配
**标签**: `mobile`, `ui`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
支持各种移动设备屏幕尺寸

## 适配内容
1. Canvas和RectTransform设置
   - 使用Canvas Scaler
   - 引用分辨率: 1080x1920

2. 刘海屏/安全区处理
   - SafeArea检测
   - UI避开非安全区域

3. 不同宽高比处理
   - 16:9 (常见手机)
   - 18:9 (高屏比手机)
   - 19.5:9 (超高屏比)
   - 甚至平板(4:3)

4. 横屏/竖屏支持
   - 当前锁定竖屏
   - 可预留横屏接口

## 测试清单
- [ ] 小屏手机 (4.7" iPhone SE)
- [ ] 普通手机 (5.5" iPhone 12)
- [ ] 大屏手机 (6.7" iPhone 12 Pro Max)
- [ ] 平板 (iPad)
- [ ] 超宽屏手机 (三星S21 Ultra)

## 验收条件
- 所有分辨率下UI显示正确
- 无文字被切割或裁剪
- 刘海屏问题解决
```

---

## Phase 9：数据持久化与存档

### Issue #27
**标题**: 玩家存档系统
**标签**: `data`, `save-system`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
实现玩家数据的本地存储和读取

## 存储数据
```csharp
[System.Serializable]
public class PlayerSaveData
{
    public int highScore; // 最高分
    public int totalCoins; // 总金币
    public List<int> unlockedSkins; // 已解锁皮肤ID列表
    public int currentSelectedSkin; // 当前选中皮肤
    public GameSettings settings; // 游戏设置
}
```

## 存储方式
- 首选: PlayerPrefs (简单数据)
- 进阶: JSON序列化到本地文件
- 可选: 云存储(Firebase)

## SaveManager实现
```csharp
public class SaveManager : MonoBehaviour
{
    public void SaveGame(PlayerSaveData data);
    public PlayerSaveData LoadGame();
    public void DeleteSaveData();
}
```

## 自动保存时机
- 游戏结束时
- 解锁新皮肤时
- 金币变化时
- 应用后台时

## 验收条件
- 数据能正确保存和读取
- 游戏重启后数据不丢失
- 没有数据损坏问题
```

---

### Issue #28
**标题**: 游戏分析系统（可选）
**标签**: `analytics`, `data`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
集成游戏分析用于了解玩家行为

## 追踪事件
1. 应用生命周期
   - FirstLaunch
   - SessionStart
   - SessionEnd

2. 游戏事件
   - GameStart
   - GameEnd (含最终分数)
   - SkinUnlocked
   - CoinSpend

3. 用户行为
   - TimeInGame
   - SkinMostUsed
   - AverageScore

## 集成选项
- Firebase Analytics（推荐）
- Unity Analytics
- Amplitude

## 实现示例
```csharp
Analytics.RecordEvent("game_over", 
    new Dictionary<string, object>
    {
        {"score", finalScore},
        {"skin_used", selectedSkin},
        {"coins_earned", coinsThisRun}
    });
```

## 数据用途
- 调整难度曲线
- 了解最受欢迎的皮肤
- 优化内购策略
- 玩家保留率分析

## 验收条件
- 事件能正确上传
- 数据在控制台正常显示
```

---

## Phase 10：内购与货币系统

### Issue #29
**标题**: Unity IAP集成
**标签**: `monetization`, `iap`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现应用内购买系统

## 产品定义
1. 金币包
   - 500 coins - $0.99
   - 2500 coins - $4.99
   - 6500 coins - $9.99

2. 皮肤包（可选）
   - 角色集合1 - $2.99
   - 角色集合2 - $2.99

## IAP集成步骤
- [ ] 导入Unity IAP插件
- [ ] 配置App Store和Google Play产品
- [ ] 实现IAPManager单例
- [ ] 处理购买、失败、恢复逻辑

## 实现框架
```csharp
public class IAPManager : MonoBehaviour
{
    public void BuyProduct(string productId);
    public event System.Action<string> OnPurchaseSuccess;
    public event System.Action<string> OnPurchaseFailed;
}
```

## 测试环境
- iOS: TestFlight / Sandbox账户
- Android: Google Play Console沙盒

## 验收条件
- 购买流程完整
- 服务器验证正确
- 金币能正确添加
- 退款处理正确
```

---

### Issue #30
**标题**: 移动广告集成
**标签**: `monetization`, `ads`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
集成激励视频广告用于获取奖励

## 广告类型
1. 激励视频
   - 游戏结束时可选观看
   - 观看完成后+50金币
   - 观看4次可解锁一个随机皮肤

2. 横幅广告（可选）
   - 菜单界面底部
   - 非侵入式

3. 插页式广告（可选）
   - 游戏结束后
   - 返回菜单时

## 广告SDK选项
- Google AdMob (推荐)
- Unity Ads
- AppLovin

## 实现逻辑
```csharp
public class AdsManager : MonoBehaviour
{
    public void ShowRewardedAd(System.Action<bool> onComplete);
}
```

## 用户体验
- 广告不强制
- 清晰的奖励展示
- 广告失败无惩罚
- 频率限制（不超烦人）

## 验收条件
- 广告能正确加载和播放
- 奖励正确发放
- 无广告时游戏正常进行
```

---

## Phase 11：测试与调试

### Issue #31
**标题**: 核心游戏玩法测试
**标签**: `testing`, `qa`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
全面测试游戏核心玩法机制

## 测试清单
### 移动系统
- [ ] 左右移动响应准确
- [ ] 跳跃高度正确
- [ ] 落地碰撞检测
- [ ] 移动动画流畅

### 饥饿系统
- [ ] 饥饿值正常递减
- [ ] 草地拾取恢复正确
- [ ] 低饥饿值警告显示
- [ ] 饿死判定准确

### 障碍物
- [ ] 5种障碍物都能生成
- [ ] 碰撞伤害判定准确
- [ ] 运动轨迹正确
- [ ] 死亡反馈正常

### 金币和道具
- [ ] 金币正常拾取
- [ ] 道具效果正常
- [ ] 无敌状态有效
- [ ] 弹跳板距离准确

### 高度系统
- [ ] 高度计算准确
- [ ] 分数正确统计
- [ ] 最高分正确保存

## 测试工具
- Unity Test Framework
- Manual QA检查表
- Profiler性能检查

## 验收条件
- 所有测试项通过
- 无关键bug
- 游戏可玩性确认
```

---

### Issue #32
**标题**: 多设备兼容性测试
**标签**: `testing`, `mobile`, `high`
**优先级**: 🟠 High
**描述**:
```
## 任务描述
在真实设备上测试游戏兼容性

## iOS测试设备
- [ ] iPhone SE (小屏, iOS 15+)
- [ ] iPhone 12 (标准屏, iOS 15+)
- [ ] iPhone 12 Pro Max (大屏, iOS 15+)
- [ ] iPad (平板, iOS 15+)

## Android测试设备
- [ ] Pixel 5 (标准安卓, Android 11+)
- [ ] 小米 (国内常见, Android 10+)
- [ ] 华为 (国内常见, Android 10+)
- [ ] 三星 (超宽屏, Android 11+)

## 测试项目
- [ ] 游戏能正常启动
- [ ] UI在各分辨率正确显示
- [ ] 输入响应正常
- [ ] 帧率稳定(45-60 FPS)
- [ ] 没有内存泄漏
- [ ] 没有崩溃错误
- [ ] 音效/音乐正常播放

## 测试工具
- Xcode (iOS真机)
- Android Studio (Android真机)
- Unity Remote (快速测试)

## 验收条件
- 至少5种设备测试通过
- 帧率稳定性> 95%
- 无致命bug
```

---

### Issue #33
**标题**: 用户体验测试与反馈收集
**标签**: `testing`, `ux`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
邀请测试者进行可玩性评估

## 测试方式
1. 内部测试 (开发团队)
   - 逐级难度体验
   - 特殊情况测试

2. Beta测试 (20-50人)
   - 通过TestFlight (iOS)
   - 通过Google Play Beta (Android)
   - 收集反馈问卷

3. 焦点小组 (核心玩家)
   - 难度评估
   - UI/UX意见
   - 美术风格反馈

## 反馈问卷
- 游戏难度: 太简单 / 适中 / 太难
- 继续游玩意愿: 不想 / 一般 / 很想
- 美术风格: 不喜欢 / 一般 / 很喜欢
- 操作流畅度: 是 / 不是
- 建议或不满: 自由填写

## 数据分析
- 平均游戏时长
- 玩家留存率 (Day 1, Day 7)
- 平均分数
- 最常用皮肤
- 常见崩溃点

## 验收条件
- 收集至少10份有效反馈
- 主要问题修复
- 玩家满意度> 70%
```

---

## Phase 12：发布与优化

### Issue #34
**标题**: App Store和Google Play准备
**标签**: `release`, `build`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
为应用商店发布做准备

## iOS准备
### 证书和配置
- [ ] 申请Apple Developer Account ($99/年)
- [ ] 生成App ID
- [ ] 创建App Store Connect应用
- [ ] 配置证书和描述文件

### App信息
- [ ] 应用名称和副标题
- [ ] 应用描述 (最多4000字符)
- [ ] 关键词 (最多100字符)
- [ ] 支持URL和隐私政策URL
- [ ] 类别选择 (Games > Action或Puzzle)

### 内容准备
- [ ] 应用截图 (5张不同屏幕)
  - 6.5" iPhone Pro Max格式
  - 高分辨率, 展示核心特性
- [ ] 应用预览视频 (15-30秒)
- [ ] 应用图标 (1024x1024)

### 版本信息
- [ ] 版本号: 1.0.0
- [ ] 构建号: 1
- [ ] 发行说明 (What's new: "首次发布")

## Android准备
### 账户和签名
- [ ] 创建Google Play Developer Account ($25)
- [ ] 生成上传密钥
- [ ] 生成应用签名密钥

### Play商店信息
- [ ] 应用名称 (最长50字符)
- [ ] 短描述 (最长80字符)
- [ ] 完整描述 (最长4000字符)
- [ ] 类别 (Games > Casual)
- [ ] 内容分级问卷

### 内容准备
- [ ] 应用截图 (4-8张)
  - 按要求的分辨率
  - PNG或JPEG格式
- [ ] 应用预览视频
- [ ] 应用图标 (512x512)

### 隐私政策
- [ ] 编写隐私政策 (涵盖数据收集、使用)
- [ ] 发布到网站
- [ ] 在商店中提供链接

## 验收条件
- 所有必需信息完成
- 截图和图标高质量
- 隐私政策清晰完整
- 本地化检查 (如适用)
```

---

### Issue #35
**标题**: 应用提交与审核
**标签**: `release`, `publish`, `critical`
**优先级**: 🔴 Critical
**描述**:
```
## 任务描述
提交游戏到应用商店

## iOS提交流程
1. [ ] 在Xcode中配置签名证书
2. [ ] 构建项目 (Product > Archive)
3. [ ] 上传到App Store Connect
4. [ ] 填写版本信息
5. [ ] 提交审核

## Android提交流程
1. [ ] 生成signed APK/AAB
   - Build > Generate Signed Bundle/APK
   - 选择release variant
2. [ ] 上传到Google Play Console
3. [ ] 填写应用信息和内容分级
4. [ ] 配置发布设置
5. [ ] 提交审核

## 审核预期时间
- iOS: 24-48小时 (通常)
- Android: 2-4小时 (通常)

## 常见拒绝原因
- 存在bug或崩溃
- 不符合商店政策
- 广告放置不当
- 描述与实际不符

## 审核通过后
- [ ] 设置发布日期或立即发布
- [ ] 监控首日下载和评分
- [ ] 关注用户反馈
- [ ] 准备更新补丁

## 验收条件
- 成功通过商店审核
- 应用上线可下载
- 没有发布后立即下架
```

---

### Issue #36
**标题**: 发布后监控和调整
**标签**: `post-launch`, `optimization`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
监控游戏上线后的表现并进行优化

## 监控指标
### 下载和安装
- [ ] 首日/周/月下载量
- [ ] 安装转化率
- [ ] 卸载率

### 用户行为
- [ ] 日活跃用户(DAU)
- [ ] 月活跃用户(MAU)
- [ ] 平均会话时长
- [ ] 平均每用户收益(ARPU)

### 技术指标
- [ ] 崩溃率 (目标< 0.1%)
- [ ] 平均启动时间
- [ ] ANR (Android Not Responding)
- [ ] 网络错误率

### 用户反馈
- [ ] 应用评分 (目标> 4.2星)
- [ ] 用户评论
- [ ] 报告的bug
- [ ] 功能建议

## 调整优化
1. **第1周**: 修复致命bug，改进性能
2. **第2-4周**: 根据反馈调整难度，优化UI
3. **第1个月后**: 计划内容更新

## 工具
- App Store Connect (iOS分析)
- Google Play Console (Android分析)
- Firebase Analytics (自定义事件)
- Crash reporting工具 (Bugly, Firebase Crashlytics)

## 验收条件
- 建立监控系统
- 定期查看数据
- 对问题快速响应
```

---

## Phase 13：后续扩展

### Issue #37
**标题**: 完整33种角色库制作计划
**标签**: `content`, `expansion`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
逐步扩展到完整33种角色库

## 分阶段计划
### 第1个月: 首发8种
- 原始山羊
- 樱花武士羊
- 夜视镜羊
- 像素羊
- 地下洞穴羊
- 冰雪羊
- 沙漠羊
- 太空羊

### 第2个月: 再加8种
- 独角兽羊
- 森林精灵羊
- 火焰地狱羊
- 海洋羊
- 骷髅羊
- 西部牛仔羊
- 忍者羊
- 阿凡达羊

### 第3个月: 再加8种
- 吸血鬼羊
- 金甲骑士羊
- 魔法师羊
- 丧尸羊
- 机器人羊
- 宇航员羊
- 恐龙羊
- 鬼魂羊

### 第4-5个月: 最后9种
- 乐高羊
- 中国羊
- 印度羊
- 阿拉伯羊
- 非洲羊
- 美国牛仔羊
- 圣诞羊
- 万圣节南瓜羊
- 彩虹独角兽羊

## 每个角色需要
- 3D模型/皮肤
- Animator动画
- 关联主题
- 解锁方式描述

## 验收条件
- 33种角色全部实现
- 每种都有独特的视觉风格
- 主题和音效搭配
```

---

### Issue #38
**标题**: 限时活动和季节性内容
**标签**: `content`, `event`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
创建限时活动增加游戏新鲜感

## 活动类型
### 节日活动
- 圣诞节 (12月)
  - 特殊主题和皮肤
  - 金币加成活动
  - 限时皮肤可获得

- 春节 (2月)
  - 中国传统主题
  - 红包金币掉落
  - 限时皮肤: 中国羊

- 万圣节 (10月)
  - 恐怖主题关卡
  - 特殊皮肤
  - 蜘蛛/南瓜障碍物

### 竞赛活动
- 周排行榜竞赛
  - 每周重置
  - 奖励排名前100玩家
  
- 主题挑战
  - 使用特定皮肤通过关卡
  - 完成特定任务获得奖励

## 实现需求
- 事件管理系统
- 时间限制检查
- 活动UI弹窗
- 奖励发放逻辑

## 验收条件
- 活动能按时启动和结束
- 奖励正确发放
- 没有活动bug
```

---

### Issue #39
**标题**: 全球排行榜系统
**标签**: `multiplayer`, `social`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现玩家排行榜，增加社交竞争

## 排行榜类型
1. 全球排行榜
   - 显示世界排名前1000
   - 每日、每周、每月刷新

2. 好友排行榜
   - 显示Facebook/Game Center好友排名
   - 实时更新

3. 皮肤排行
   - 统计使用最多的皮肤
   - 了解玩家偏好

## 数据同步
- 每局游戏后上传分数
- 使用Firebase Realtime Database或Leaderboard API
- 离线模式支持

## UI展示
- 排行榜界面显示排名
- 用户头像/昵称
- 分数和排名
- 与玩家分数对比

## 验收条件
- 排行榜能正确上传和显示
- 没有作弊检测问题
- 加载速度快
```

---

### Issue #40
**标题**: 每日任务和登录奖励
**标签**: `gameplay`, `engagement`, `medium`
**优先级**: 🟡 Medium
**描述**:
```
## 任务描述
实现每日奖励系统提高玩家粘性

## 功能
### 登录奖励
- 每天首次启动游戏
- 领取固定奖励 (金币)
- 7日连续登录有加倍奖励

### 每日任务
- 每日3个任务
  - 爬升1000高度
  - 收集50枚金币
  - 使用3个不同皮肤

- 任务完成奖励 (金币和经验)
- 完成全部任务额外奖励

### 任务重置
- UTC 0:00重置
- 在本地存储任务状态

## 实现框架
```csharp
[System.Serializable]
public class DailyTask
{
    public int id;
    public string description;
    public int targetValue;
    public int currentValue;
    public int reward;
    public bool completed;
}
```

## UI设计
- 主菜单显示登录提示
- 任务列表和进度条
- 领取奖励按钮

## 验收条件
- 任务能正确生成和重置
- 奖励能正确发放
- 登录信息能正确存储
```

---

## 标签说明

| 标签 | 含义 |
|-----|------|
| `setup` | 项目初始化和基础设置 |
| `core-system` | 核心游戏系统 |
| `architecture` | 代码架构和设计 |
| `gameplay` | 游戏玩法机制 |
| `input` | 输入系统 |
| `art` | 美术和视觉 |
| `animation` | 动画系统 |
| `ui` | 用户界面 |
| `menu` | 菜单界面 |
| `hud` | 游戏内信息显示 |
| `audio` | 音频和音效 |
| `theme` | 主题系统 |
| `vfx` | 视觉特效 |
| `obstacles` | 障碍物系统 |
| `physics` | 物理和碰撞 |
| `mechanic` | 游戏机制 |
| `economy` | 经济系统 |
| `power-up` | 道具系统 |
| `mobile` | 移动平台 |
| `optimization` | 性能优化 |
| `testing` | 测试和QA |
| `qa` | 质量保证 |
| `data` | 数据系统 |
| `save-system` | 存档系统 |
| `analytics` | 分析系统 |
| `monetization` | 商业化 |
| `iap` | 应用内购买 |
| `ads` | 广告系统 |
| `release` | 发布准备 |
| `build` | 构建和编译 |
| `publish` | 发布上线 |
| `post-launch` | 发布后 |
| `expansion` | 功能扩展 |
| `content` | 内容 |
| `event` | 事件活动 |
| `social` | 社交功能 |
| `engagement` | 用户参与度 |

---

## 优先级说明

- 🔴 **Critical**: 必须完成，否则游戏不可玩
- 🟠 **High**: 重要功能，发布前必须完成
- 🟡 **Medium**: 提升体验的功能，可在发布后迭代
- 🟢 **Low**: 锦上添花，可选择性完成

---

## 导入到GitHub

### 方式1: 手动创建
1. 打开GitHub项目 Issues
2. 点击 "New Issue"
3. 复制上面的标题、标签、描述
4. 逐个创建

### 方式2: 使用GitHub CLI
```bash
gh issue create --title "项目初始化与文件夹结构" \
  --body "..." \
  --label "setup,critical"
```

### 方式3: 创建Project
1. 在项目中创建 "Project"
2. 选择 "Table" 或 "Board" 视图
3. 添加自定义列: To Do, In Progress, Done
4. 将Issues拖到相应列

