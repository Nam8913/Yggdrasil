namespace BehaviorTree
{
    /// <summary>
    /// Inverts the child's result: Success becomes Failure, Failure becomes Success.
    /// Đảo ngược kết quả của con: Success thành Failure, Failure thành Success.
    ///
    /// Running is passed through unchanged.
    /// Running được giữ nguyên không đổi.
    ///
    /// Usage: "do something UNLESS condition is true" or "do something IF condition is false".
    /// Sử dụng: "làm gì đó TRỪ KHI điều kiện đúng" hoặc "làm gì đó NẾU điều kiện sai".
    /// </summary>
    public class InverterNode : DecoratorNode
    {
        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();
            return Invert(state);
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            observer.Descend();
            var state = Child.Evaluate(ref observer);
            observer.Ascend();
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
