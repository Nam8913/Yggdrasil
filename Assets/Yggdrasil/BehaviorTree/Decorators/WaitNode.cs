using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Waits for a specified duration before executing the child.
    /// Chờ một khoảng thời gian nhất định trước khi thực thi con.
    ///
    /// While waiting, returns Running. After the wait completes,
    /// delegates to the child normally.
    /// Trong khi chờ, trả về Running. Sau khi chờ xong,
    /// ủy quyền cho con bình thường.
    ///
    /// Usage: delay before next action, patrol pause, animation timing.
    /// Sử dụng: trì hoãn trước hành động tiếp theo, tạm dừng tuần tra, timing animation.
    /// </summary>
    public class WaitNode : DecoratorNode
    {
        // Seconds to wait before executing the child
        // Số giây chờ trước khi thực thi con
        public float WaitSeconds { get; set; } = 1f;
        public BHState ReturnIfChildNull { get; set; } = BHState.Success;

        private float _startTime;
        private bool _waiting;

        protected override void OnEnter()
        {
            _startTime = Time.time;
            _waiting = true;
        }

        protected override BHState OnUpdate()
        {
            if (_waiting)
            {
                if (Time.time - _startTime < WaitSeconds)
                    return BHState.Running;

                _waiting = false;
            }

            if(Child == null)
                return ReturnIfChildNull;

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (_waiting)
            {
                if (Time.time - _startTime < WaitSeconds)
                    return BHState.Running;

                _waiting = false;
            }

            if(Child == null)
                return ReturnIfChildNull;

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            if (_waiting)
                return BHState.Running;

            if(Child == null)
                return ReturnIfChildNull;

            return Child.Execute();
        }

        protected override void OnReset()
        {
            base.OnReset();
            _waiting = false;
        }
    }
}
