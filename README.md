# Unity ARPG Combat Demo

##  Project Overview
A third-person ARPG combat demo developed independently using Unity and C#.  
The project focuses on a Root Motion-driven combat system and an event-driven architecture, including character combat, camera control, and basic client-side framework design.

##  Features
- Root Motion-based character movement and combat system  
- Attack, hit reaction, and combo logic implementation  
- Cinemachine-based camera system (state switching, screen shake, FOV change)  
- Event-driven architecture for module decoupling  
- UI system based on finite state machine  
- Generic object pool for performance optimization  
- **Behavior Tree-driven enemy AI system**  

##  Tech Stack
- Unity (C#)  
- Animator (Root Motion)  
- Cinemachine  
- ScriptableObject (Event System)  
- NavMesh (AI Pathfinding)  

##  Highlights
- Implemented a Root Motion-driven combat system to ensure animation-motion consistency  
- Designed a modular client architecture with event-driven communication  
- Applied object pooling to reduce GC allocation and improve runtime performance  
- **Designed a custom behavior tree framework (Selector/Sequence/PreCondition/Action) for enemy AI**
- **Implemented priority-based behavior switching: Hit > Attack > Interim > Vigilant > Chase > Patrol > Idle**
- **Introduced PreConditionNode for per-frame condition guarding, separating action logic from precondition checks**  

---

## 📌 项目简介
本项目为基于 Unity 与 C# 独立开发的第三人称 ARPG 战斗 Demo，  
围绕 Root Motion 动作驱动与事件驱动架构构建，实现角色战斗、相机控制及基础客户端架构设计。

## ⭐ 技术亮点
- 基于 Root Motion 的角色控制与战斗系统  
- 基于 Cinemachine 的多状态相机系统  
- 事件驱动架构实现模块解耦  
- 通用对象池优化性能  
- **自定义行为树框架驱动敌人 AI（Selector / Sequence / PreCondition / Action）**
- **优先级行为切换：受击 > 攻击 > 过渡 > 警戒 > 追击 > 巡逻 > 待机**
- **PreConditionNode 实现持续状态的条件守卫，每帧校验前置条件**  

---

##  How to Run
1. Open the project with Unity Hub  
2. Load the main scene  
3. Click Play  
