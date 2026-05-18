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

## ⭐ 功能特性
- 基于 Root Motion 的角色移动与战斗系统  
- 攻击、受击与连招逻辑实现  
- 基于 Cinemachine 的相机系统（状态切换、屏幕震动、FOV 变更）  
- 事件驱动架构实现模块解耦  
- 基于有限状态机的 UI 系统  
- 通用对象池优化性能  
- **行为树驱动的敌人 AI 系统**  

## 🛠 技术栈
- Unity (C#)  
- Animator (Root Motion)  
- Cinemachine  
- ScriptableObject (Event System)  
- NavMesh (AI 寻路)  

## 💡 核心亮点
- 实现 Root Motion 驱动战斗系统，保证动画与位移一致性  
- 设计模块化客户端架构，通过事件驱动实现模块间通信  
- 应用对象池降低 GC 分配，提升运行时性能  
- **设计自定义行为树框架（Selector / Sequence / PreCondition / Action）驱动敌人 AI**
- **实现基于优先级的行为切换：受击 > 攻击 > 过渡 > 警戒 > 追击 > 巡逻 > 待机**
- **引入 PreConditionNode 实现持续状态的条件守卫，将动作逻辑与前置条件检查分离**  

---

##  How to Run
1. Open the project with Unity Hub  
2. Load the main scene  
3. Click Play  
