namespace BehaviorTree
{
    /// <summary>
    /// Base class for nodes with a single child. Decorators modify the child's
    /// behavior (invert, repeat, guard, etc.) or add logic around it.
    /// Lớp cơ sở cho các node có một con. Decorator sửa đổi hành vi của con
    /// (đảo ngược, lặp lại, bảo vệ, v.v.) hoặc thêm logic xung quanh nó.
    /// </summary>
    public abstract class DecoratorNode : NodeBT
    {
        // The single child node this decorator wraps
        // Node con duy nhất mà decorator này bọc
        public NodeBT Child { get; set; }

        protected override void OnInitialize()
        {
            Child?.Initialize(Blackboard);
        }

        protected override void OnReset()
        {
            Child?.Reset();
        }

        // Abort propagates to child, ensuring clean cancellation
        // Abort lan truyền xuống con, đảm bảo hủy sạch sẽ
        public override void Abort()
        {
            Child?.Abort();
            base.Abort();
        }
    }
}
