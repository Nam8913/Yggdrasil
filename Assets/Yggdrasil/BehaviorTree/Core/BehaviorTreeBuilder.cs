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

        public BehaviorTreeBuilder Service(float interval, System.Action service)
        {
            UnityEngine.Debug.LogWarning("[BehaviorTreeBuilder] Service() is not yet implemented. Service node skipped.");
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
