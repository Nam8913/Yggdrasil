using System.Collections.Generic;

namespace BehaviorTree
{
    public abstract class CompositeNode : NodeBT
    {
        protected readonly List<NodeBT> Children = new List<NodeBT>();
        protected int CurrentChildIndex;

        public IReadOnlyList<NodeBT> GetChildren()
        {
            return Children;
        }

        public void AddChild(NodeBT child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        protected override void OnInitialize()
        {
            foreach (var child in Children)
            {
                child?.Initialize(Blackboard);
            }
        }

        protected override void OnReset()
        {
            CurrentChildIndex = 0;
            foreach (var child in Children)
            {
                child?.Reset();
            }
        }
    }
}
