# Unity ARPG Combat Demo

##  Project Overview
A third-person ARPG combat demo developed independently using Unity and C#.  
Features a Root Motion-driven combat system, EventBus architecture, MVC inventory system, Addressables asset loading, and behavior tree AI.

##  Features
- Root Motion-based character movement and combat system  
- Attack, hit reaction, and combo logic implementation  
- Cinemachine-based camera system (state switching, screen shake, FOV change)  
- **Unified EventBus — enum-keyed delegate dictionary replacing ScriptableObject event assets**  
- UI system based on finite state machine  
- **MVC inventory system (pickup, discard, drag-swap, sort)**  
- **Addressables asset bundle loading with Bootstrap async flow**  
- **Animation state machine refactored: CrossFade direct control + SO config**  
- Generic object pool for performance optimization  
- **Behavior Tree-driven enemy AI system**  

##  Tech Stack
- Unity (C#)  
- Animator (Root Motion / CrossFade)  
- Cinemachine  
- **EventBus (global delegate dictionary)**  
- **Addressables (AB loading)**  
- NavMesh (AI Pathfinding)  

##  Highlights
- Implemented a Root Motion-driven combat system to ensure animation-motion consistency  
- **Replaced Animator parameter-driven transitions with CrossFade + ScriptableObject config**  
- **Built a unified EventBus singleton for module decoupling — zero Inspector drag dependencies**  
- **Designed MVC inventory: M (indexer auto-fires events), C (Add/Remove/Swap/Sort), V (object pool + drag-drop)**  
- **Integrated Addressables: BootScene → async load SOs → UI → MainScene → Player → Enemies**  
- Applied object pooling to reduce GC allocation and improve runtime performance  
- **Designed custom behavior tree framework (Selector/Sequence/PreCondition/Action) for enemy AI**
- **Implemented priority-based behavior: Hit > Attack > Interim > Vigilant > Chase > Patrol > Idle**
- **PreConditionNode for per-frame condition guarding, separating action logic from precondition checks**  

---

## 📌 项目简介
基于 Unity + C# 的第三人称 ARPG 战斗 Demo，覆盖 Root Motion 动作驱动、EventBus 事件总线、MVC 背包、Addressables 加载和行为树 AI。

## ⭐ 功能特性
- 基于 Root Motion 的角色移动与战斗系统  
- 攻击、受击与连招逻辑实现  
- 基于 Cinemachine 的相机系统（状态切换、屏幕震动、FOV 变更）  
- **统一 EventBus 事件总线，枚举 key + Delegate 字典管理全部事件**  
- 基于有限状态机的 UI 系统  
- **MVC 背包系统：拾取宝箱、丢弃、拖拽交换、一键整理**  
- **Addressables AB 包加载：Bootstrap 异步启动 → 按需加载场景/角色/UI/SO**  
- **动画状态机重构：CrossFade 直接控制 + SO 资产配置过渡时间**  
- 通用对象池优化性能  
- **行为树驱动的敌人 AI 系统**  

## 🛠 技术栈
- Unity (C#)  
- Animator (Root Motion / CrossFade)  
- Cinemachine  
- **EventBus（全局事件字典）**  
- **Addressables（AB 包加载）**  
- NavMesh (AI 寻路)  

## 💡 核心亮点
- 实现 Root Motion 驱动战斗系统，保证动画与位移一致性  
- **CrossFade + SO 资产替代 Animator 参数驱动，代码直接控制动画播放**  
- **构建 EventBus 单例消除跨模块 Inspector 拖拽依赖**  
- **设计 MVC 背包：M 层索引器 set 自动发事件，C 层业务逻辑，V 层对象池按需生成**  
- **Addressables 异步加载 + Bootstrap 统一入口**  
- 设计模块化客户端架构，事件驱动实现模块间通信  
- 应用对象池降低 GC 分配，提升运行时性能  
- **自定义行为树框架（Selector / Sequence / PreCondition / Action）驱动敌人 AI**
- **优先级行为切换：受击 > 攻击 > 过渡 > 警戒 > 追击 > 巡逻 > 待机**
- **PreConditionNode 实现持续状态条件守卫**  

---

##  How to Run
1. Open the project with Unity Hub  
2. Open `Assets/Scenes/BootScene.unity` as the startup scene  
3. Click Play  
