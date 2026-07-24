using System;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class BehaviorTreeBuilder
    {
        private readonly Blackboard _blackboard;
        private readonly Stack<NodeBT> _nodeStack = new Stack<NodeBT>();
        private RootNode _root;

        public BehaviorTreeBuilder(Blackboard blackboard = null)
        {
            _blackboard = blackboard ?? new Blackboard();
        }

        public BehaviorTreeBuilder Root()
        {
            _root = new RootNode();
            _nodeStack.Push(_root);
            return this;
        }

        public BehaviorTreeBuilder Sequence(string name = null)
        {
            var node = new SequenceNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Selector(string name = null)
        {
            var node = new SelectorNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder RandomSequence()
        {
            var node = new RandomSequenceNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder RandomSelector()
        {
            var node = new RandomSelectorNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Nand()
        {
            var node = new NandNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Nor()
        {
            var node = new NorNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Xor()
        {
            var node = new XorNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Xnor()
        {
            var node = new XnorNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Parallel(ParallelPolicy policy = ParallelPolicy.RequireAll)
        {
            var node = new ParallelNode { Policy = policy };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Inverter()
        {
            var node = new InverterNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Succeeder()
        {
            var node = new SucceederNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Failer()
        {
            var node = new FailerNode();
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Cooldown(float seconds)
        {
            var node = new CooldownNode { CooldownSeconds = seconds };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Repeater(int maxRepeats = -1)
        {
            var node = new RepeaterNode { MaxRepeats = maxRepeats };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder TimeLimit(float seconds)
        {
            var node = new TimeLimitNode { LimitSeconds = seconds };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Wait(float seconds)
        {
            var node = new WaitNode { WaitSeconds = seconds };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder WaitUntil(Func<bool> condition)
        {
            var node = new WaitUntilNode(condition);
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder RepeatUntil(Func<bool> condition)
        {
            var node = new RepeatUntilNode { Condition = condition };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder RepeatUntilState(BHState desiredState = BHState.Success)
        {
            var node = new RepeatUntilStateNode { Value = desiredState };
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Guard(Func<bool> condition, int priority = 0)
        {
            var node = new GuardNode(condition, null, priority);
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder Action(ActionNode action)
        {
            AddToParent(action);
            return this;
        }

        public BehaviorTreeBuilder Condition(ConditionNode condition)
        {
            AddToParent(condition);
            return this;
        }

        public BehaviorTreeBuilder Condition(Func<bool> condition)
        {
            AddToParent(new ActionConditionNode(condition));
            return this;
        }

        public BehaviorTreeBuilder Service(float interval, Action service)
        {
            var node = new ActionServiceNode(service, interval);
            AddToParent(node);
            _nodeStack.Push(node);
            return this;
        }

        public BehaviorTreeBuilder End()
        {
            if (_nodeStack.Count <= 1)
            {
                UnityEngine.Debug.LogWarning("[BehaviorTreeBuilder] End() called with no open composite/decorator to close.");
                return this;
            }

            _nodeStack.Pop();
            return this;
        }

        public BehaviorTreeBuilder Action<T>(T actionNode) where T : ActionNode
        {
            AddToParent(actionNode);
            return this;
        }

        public BehaviorTreeBuilder Composite<T>(T compositeNode) where T : CompositeNode
        {
            AddToParent(compositeNode);
            _nodeStack.Push(compositeNode);
            return this;
        }

        public BehaviorTreeBuilder Decorator<T>(T decoratorNode) where T : DecoratorNode
        {
            AddToParent(decoratorNode);
            _nodeStack.Push(decoratorNode);
            return this;
        }

        public BehaviorTreeBuilder Condition<T>(T conditionNode) where T : ConditionNode
        {
            AddToParent(conditionNode);
            return this;
        }

        public RootNode Build()
        {
            if (_root == null)
            {
                UnityEngine.Debug.LogError("[BehaviorTreeBuilder] Build() called without Root(). Call Root() first.");
                return null;
            }

            if (_nodeStack.Count > 1)
            {
                UnityEngine.Debug.LogWarning($"[BehaviorTreeBuilder] Build() called with {_nodeStack.Count - 1} unclosed nodes. Missing End() calls.");
            }

            _root.Initialize(_blackboard);
            return _root;
        }

        public (RootNode root, Blackboard blackboard) BuildWithBlackboard()
        {
            if (_root == null)
            {
                UnityEngine.Debug.LogError("[BehaviorTreeBuilder] BuildWithBlackboard() called without Root(). Call Root() first.");
                return (null, _blackboard);
            }

            if (_nodeStack.Count > 1)
            {
                UnityEngine.Debug.LogWarning($"[BehaviorTreeBuilder] BuildWithBlackboard() called with {_nodeStack.Count - 1} unclosed nodes. Missing End() calls.");
            }

            _root.Initialize(_blackboard);
            return (_root, _blackboard);
        }

        private void AddToParent(NodeBT node)
        {
            if (_nodeStack.Count == 0)
                throw new InvalidOperationException("No parent node. Call Root() first.");

            var parent = _nodeStack.Peek();

            if (parent is CompositeNode composite)
                composite.AddChild(node);
            else if (parent is DecoratorNode decorator)
                decorator.Child = node;
        }
    }
}
