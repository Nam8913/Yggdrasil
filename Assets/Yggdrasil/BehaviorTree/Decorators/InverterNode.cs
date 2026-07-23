namespace BehaviorTree
{
    /// <summary>
    /// Inverts the child's result: Success becomes Failure, Failure becomes Success.
    /// Đảo ngược kết quả của con: Success thành Failure, Failure thành Success.
    ///
    /// Running is passed through unchanged.
    /// Running được giữ nguyên không đổi.
    ///
    /// Usage: "do something UNLESS condition is true"
    /// Sử dụng: "làm gì đó TRỪ KHI điều kiện đúng"
    /// </summary>
    public class InverterNode : DecoratorNode
    {
        protected override BHState OnUpdate()
        {
            var state = Child.Tick();
            return Invert(state);
        }

        protected override BHState OnEvaluate()
        {
            var state = Child.Evaluate();
            return Invert(state);
        }

        protected override BHState OnExecute()
        {
            return Invert(Child.Execute());
        }

        private BHState Invert(BHState state)
        {
            return state switch
            {
                BHState.Success => BHState.Failure,
                BHState.Failure => BHState.Success,
                _ => BHState.Running
            };
        }
    }
}
