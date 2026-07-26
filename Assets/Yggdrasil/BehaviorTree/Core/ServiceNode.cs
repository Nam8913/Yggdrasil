using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Abstract decorator that executes a service action at regular intervals.
    /// Decorator trừu tượng thực thi hành động dịch vụ theo khoảng thời gian đều đặn.
    ///
    /// Use cases: periodic perception updates, cooldown timers, state polling.
    /// Trường hợp sử dụng: cập nhật cảm nhận định kỳ, bộ đếm thời gian chờ, kiểm tra trạng thái.
    ///
    /// The service runs BEFORE the child node each time the interval elapses.
    /// Dịch vụ chạy TRƯỚC node con mỗi khi khoảng thời gian hết hạn.
    /// </summary>
    public abstract class ServiceNode : DecoratorNode
    {
        // Interval between service executions (in seconds)
        // Khoảng thời gian giữa các lần thực thi dịch vụ (tính bằng giây)
        public float ServiceInterval { get; set; } = 0.5f;
        private float _lastServiceTime;

        protected override void OnEnter()
        {
            _lastServiceTime = Time.time;
            ExecuteService();
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Time.time - _lastServiceTime >= ServiceInterval)
            {
                ExecuteService();
                _lastServiceTime = Time.time;
            }

            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (Time.time - _lastServiceTime >= ServiceInterval)
            {
                ExecuteService();
                _lastServiceTime = Time.time;
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

        // Subclasses implement this to define what the service does
        // Các lớp con triển khai phương thức này để xác định dịch vụ làm gì
        protected abstract void ExecuteService();
    }
}
