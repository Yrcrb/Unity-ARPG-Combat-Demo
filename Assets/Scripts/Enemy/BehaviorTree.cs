using System;
using System.Collections.Generic;

/// <summary>
/// 行为树节点执行后只会返回三种标准状态。
/// </summary>
public enum NodeState
{
    // 当前节点已经顺利完成。
    Success,

    // 当前节点条件不满足或执行失败。
    Failure,

    // 当前节点还在持续执行，需要下一帧继续 Tick。
    Running
}

public class BasicsNode
{
    // 当前节点持有的所有子节点。
    protected readonly List<BasicsNode> children = new List<BasicsNode>();

    // 把一个子节点挂到当前节点下。
    public void AddChild(BasicsNode node)
    {
        if (node != null)
        {
            children.Add(node);
        }
    }

    // 默认 Tick 直接返回 Success，实际逻辑由子类覆写。
    public virtual NodeState Tick()
    {
        return NodeState.Success;
    }

    // 默认 Reset 会递归重置整棵子树。
    public virtual void Reset()
    {
        foreach (var child in children)
        {
            child.Reset();
        }
    }
}

public class SelectorNode : BasicsNode
{
    // 当前正在 Running 的子节点索引。
    private int runningChildIndex = -1;

    // 依次尝试子节点，直到有一个成功或正在执行。
    public override NodeState Tick()
    {
        int previousRunningIndex = runningChildIndex;
        runningChildIndex = -1;

        for (int i = 0; i < children.Count; i++)
        {
            NodeState state = children[i].Tick();
            if (state == NodeState.Failure)
            {
                continue;
            }

            if (previousRunningIndex != -1 && previousRunningIndex != i)
            {
                children[previousRunningIndex].Reset();
            }

            if (state == NodeState.Running)
            {
                runningChildIndex = i;
            }

            return state;
        }

        if (previousRunningIndex != -1)
        {
            children[previousRunningIndex].Reset();
        }

        return NodeState.Failure;
    }

    // Selector 被打断时会重置当前正在运行的分支。
    public override void Reset()
    {
        if (runningChildIndex != -1)
        {
            children[runningChildIndex].Reset();
        }

        runningChildIndex = -1;
        base.Reset();
    }
}

public class SequenceNode : BasicsNode
{
    // 当前正在 Running 的子节点索引。
    private int runningChildIndex = -1;

    // 依次执行子节点，只要有一个失败就整体失败。
    public override NodeState Tick()
    {
        int startIndex = runningChildIndex >= 0 ? runningChildIndex : 0;

        for (int i = startIndex; i < children.Count; i++)
        {
            NodeState state = children[i].Tick();
            if (state == NodeState.Failure)
            {
                runningChildIndex = -1;
                return NodeState.Failure;
            }

            if (state == NodeState.Running)
            {
                runningChildIndex = i;
                return NodeState.Running;
            }
        }

        runningChildIndex = -1;
        return NodeState.Success;
    }

    // Sequence 被打断时会重置当前正在运行的分支。
    public override void Reset()
    {
        if (runningChildIndex != -1)
        {
            children[runningChildIndex].Reset();
        }

        runningChildIndex = -1;
        base.Reset();
    }
}

public class Condition : BasicsNode
{
    // 外部传进来的条件委托。
    private readonly Func<bool> condition;

    // 用一个布尔委托构造条件节点。
    public Condition(Func<bool> condition)
    {
        this.condition = condition;
    }

    // 条件节点只负责把布尔结果转成 Success 或 Failure。
    public override NodeState Tick()
    {
        bool result = condition.Invoke();
        return result ? NodeState.Success : NodeState.Failure;
    }
}

public class ActionNode : BasicsNode
{
    // 动作开始时执行一次的入口。
    private readonly Action onEnter;

    // 动作持续执行时每帧调用的入口。
    private readonly Func<NodeState> onTick;

    // 动作结束或被打断时执行一次的出口。
    private readonly Action onExit;

    // 当前动作是否已经进入过 Enter 阶段。
    private bool hasEntered;

    // 只传 Tick 的简化构造函数。
    public ActionNode(Func<NodeState> onTick) : this(null, onTick, null)
    {
    }

    // 完整构造函数，允许外部分别绑定 Enter、Tick、Exit。
    public ActionNode(Action onEnter, Func<NodeState> onTick, Action onExit)
    {
        this.onEnter = onEnter;
        this.onTick = onTick;
        this.onExit = onExit;
    }

    // 第一次 Tick 会先走 Enter，之后持续走 Tick，结束时自动调用 Exit。
    public override NodeState Tick()
    {
        if (!hasEntered)
        {
            onEnter?.Invoke();
            hasEntered = true;
        }

        NodeState state = onTick != null ? onTick.Invoke() : NodeState.Success;
        if (state != NodeState.Running)
        {
            onExit?.Invoke();
            hasEntered = false;
        }

        return state;
    }

    // 动作被更高优先级分支打断时也会补一次 Exit。
    public override void Reset()
    {
        if (hasEntered)
        {
            onExit?.Invoke();
            hasEntered = false;
        }
    }
}

public class PreConditionNode : BasicsNode
{
    private bool isActionActive;

    public PreConditionNode(BasicsNode condition, ActionNode action)
    {
        AddChild(condition);
        AddChild(action);
    }

    public override NodeState Tick()
    {
        NodeState conditionState = children[0].Tick();

        if (conditionState == NodeState.Failure)
        {
            if (isActionActive)
            {
                children[1].Reset();
                isActionActive = false;
            }

            return NodeState.Failure;
        }

        NodeState actionState = children[1].Tick();
        isActionActive = actionState == NodeState.Running;
        return actionState;
    }

    public override void Reset()
    {
        if (isActionActive)
        {
            children[1].Reset();
            isActionActive = false;
        }

        base.Reset();
    }
}
