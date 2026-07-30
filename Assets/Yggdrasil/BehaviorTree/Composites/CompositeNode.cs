using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// Base class for nodes that have multiple children.
    /// Lớp cơ sở cho các node có nhiều node con.
    ///
    /// Composites control the flow of execution through their children
    /// (sequence, selector, parallel, etc.).
    /// Composite kiểm soát luồng thực thi thông qua các node con
    /// (sequence, selector, parallel, v.v.).
    /// </summary>
    public abstract class CompositeNode : NodeBT
    {
        // List of child nodes
        // Danh sách các node con
        protected readonly List<NodeBT> Children = new List<NodeBT>();

        // Index of the currently active child node
        // Chỉ số của node con đang hoạt động
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

        // Initialize all children with the shared blackboard
        // Khởi tạo tất cả con với blackboard dùng chung
        protected override void OnInitialize()
        {
            foreach (var child in Children)
            {
                child?.Initialize(Blackboard);
            }
        }

        // Reset child index and all children
        // Reset chỉ số child và tất cả các con
        protected override void OnReset()
        {
            CurrentChildIndex = 0;
            foreach (var child in Children)
            {
                child?.Reset();
            }
        }

        // Abort propagates to all children, ensuring clean cancellation
        // Abort lan truyền xuống tất cả con, đảm bảo hủy sạch sẽ
        public override void Abort()
        {
            CurrentChildIndex = 0;
            foreach (var child in Children)
            {
                child?.Abort();
            }
            base.Abort();
        }
    }
}
