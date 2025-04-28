using System;
using System.Collections.Generic;
using UnityEngine;

// Behavior tree status
public enum BehaviorNodeStatus
{
    Success,
    Failure,
    Running
}

// Base behavior node
public abstract class BehaviorNode
{
    public abstract BehaviorNodeStatus Update();
}

// Behavior tree
public class BehaviorTree
{
    private BehaviorNode rootNode;

    public void SetRootNode(BehaviorNode node)
    {
        rootNode = node;
    }

    public BehaviorNodeStatus Update()
    {
        if (rootNode != null)
            return rootNode.Update();
        return BehaviorNodeStatus.Failure;
    }
}

// Sequence node - executes children in order, stops on first failure
public class SequenceNode : BehaviorNode
{
    private List<BehaviorNode> children = new List<BehaviorNode>();

    public void AddChild(BehaviorNode node)
    {
        children.Add(node);
    }

    public override BehaviorNodeStatus Update()
    {
        foreach (var child in children)
        {
            BehaviorNodeStatus status = child.Update();

            if (status != BehaviorNodeStatus.Success)
                return status;
        }

        return BehaviorNodeStatus.Success;
    }
}

// Selector node - executes children in order, stops on first success
public class SelectorNode : BehaviorNode
{
    private List<BehaviorNode> children = new List<BehaviorNode>();

    public void AddChild(BehaviorNode node)
    {
        children.Add(node);
    }

    public override BehaviorNodeStatus Update()
    {
        foreach (var child in children)
        {
            BehaviorNodeStatus status = child.Update();

            if (status != BehaviorNodeStatus.Failure)
                return status;
        }

        return BehaviorNodeStatus.Failure;
    }
}

// Action node - executes a function
public class ActionNode : BehaviorNode
{
    private Func<BehaviorNodeStatus> action;

    public ActionNode(Func<BehaviorNodeStatus> action)
    {
        this.action = action;
    }

    public override BehaviorNodeStatus Update()
    {
        return action();
    }
}

// Check bool node - checks a boolean condition
public class CheckBoolNode : BehaviorNode
{
    private Func<bool> condition;

    public CheckBoolNode(Func<bool> condition)
    {
        this.condition = condition;
    }

    public override BehaviorNodeStatus Update()
    {
        return condition() ? BehaviorNodeStatus.Success : BehaviorNodeStatus.Failure;
    }
}

// Check distance node - checks if a distance is within range
public class CheckDistanceNode : BehaviorNode
{
    private Func<float> getDistance;
    private float minDistance;
    private float maxDistance;

    public CheckDistanceNode(Func<float> getDistance, float minDistance, float maxDistance)
    {
        this.getDistance = getDistance;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    public override BehaviorNodeStatus Update()
    {
        float distance = getDistance();
        return (distance >= minDistance && distance <= maxDistance) ? 
            BehaviorNodeStatus.Success : BehaviorNodeStatus.Failure;
    }
}
