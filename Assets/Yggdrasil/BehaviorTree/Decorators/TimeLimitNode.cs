using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Aborts the child if it doesn't complete within the time limit.
    /// Hủy bỏ con nếu nó không hoàn thành trong giới hạn thời gian.
    ///
    /// While within the time limit, the child runs normally.
    /// When time expires, the child is aborted and this node returns Failure.
    /// Trong giới hạn thời gian, con chạy bình thường.
    /// Khi hết thời gian, con bị hủy và node này trả về Failure.
    ///
    /// Usage: patrol timeout, search timeout, action time budget.
    /// Sử dụng: hết thời gian tuần tra, hết thời gian tìm kiếm, ngân sách thời gian hành động.
    /// </summary>
    public class TimeLimitNode : DecoratorNode
    {
        // Maximum seconds the child is allowed to run
        // Số giây tối đa mà con được phép chạy
        public float LimitSeconds { get; set; } = 5f;

        private float _startTime;

        protected override void OnEnter()
        {
            _startTime = Time.time;
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Time.time - _startTime >= LimitSeconds)
            {
                Child.Abort();
                return BHState.Failure;
            }

            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (Time.time - _startTime >= LimitSeconds)
            {
                Child.Abort();
                return BHState.Failure;
            }

            observer.Descend();
            var state = Child.Evaluate(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }
    }
}
