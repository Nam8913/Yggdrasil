namespace BehaviorTree
{
    public abstract class DecoratorNode : NodeBT
    {
        public NodeBT Child { get; set; }

        protected override void OnInitialize()
        {
            Child?.Initialize(Blackboard);
        }

        protected override void OnReset()
        {
            Child?.Reset();
        }

        public override void Abort()
        {
            Child?.Abort();
            base.Abort();
        }
    }
}
