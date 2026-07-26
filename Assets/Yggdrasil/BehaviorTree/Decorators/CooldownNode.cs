using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Prevents the child from executing more often than the cooldown interval.
    /// Ngăn con thực thi quá thường xuyên so với khoảng thời gian chờ.
    ///
    /// While on cooldown, returns Failure immediately without ticking the child.
    /// Trong thời gian chờ, trả về Failure ngay lập tức mà không tick con.
    ///
    /// The cooldown starts AFTER the child completes (Success or Failure).
    /// Thời gian chờ bắt đầu SAU khi con hoàn thành (Success hoặc Failure).
    ///
    /// Usage: attack cooldown, ability cooldown, rate limiting.
    /// Sử dụng: thời gian chờ tấn công, thời gian chờ kỹ năng, giới hạn tần suất.
    /// </summary>
    public class CooldownNode : DecoratorNode
    {
        // Minimum seconds between executions
        // Số giây tối thiểu giữa các lần thực thi
        public float CooldownSeconds { get; set; } = 1f;

        // Timestamp of the last successful/failed execution
        // Thời điểm của lần thực thi thành công/thất bại cuối cùng
        private float _lastExecuteTime = -999f;

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Time.time - _lastExecuteTime < CooldownSeconds)
                return BHState.Failure;

            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();

            if (state != BHState.Running)
                _lastExecuteTime = Time.time;

            return state;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (Time.time - _lastExecuteTime < CooldownSeconds)
                return BHState.Failure;

            observer.Descend();
            var state = Child.Evaluate(ref observer);
            observer.Ascend();

            if (state != BHState.Running)
                _lastExecuteTime = Time.time;

            return state;
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }

        // Reset cooldown so the child can execute immediately on next activation
        // Reset thời gian chờ để con có thể thực thi ngay lập tức khi kích hoạt tiếp
        protected override void OnReset()
        {
            base.OnReset();
            _lastExecuteTime = -999f;
        }
    }
}
