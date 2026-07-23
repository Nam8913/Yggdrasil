namespace BehaviorTree
{
    /// <summary>
    /// Stops all movement by setting Rigidbody2D velocity to zero.
    /// Dừng mọi chuyển động bằng cách đặt velocity Rigidbody2D về không.
    ///
    /// Always returns Success immediately.
    /// Luôn trả về Success ngay lập tức.
    /// </summary>
    public class StopAction : ActionNode
    {
        protected override BHState OnUpdate()
        {
            return Stop();
        }

        protected override BHState OnExecute()
        {
            return Stop();
        }

        private BHState Stop()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return BHState.Failure;

            var rb = self.GetComponent<UnityEngine.Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = UnityEngine.Vector2.zero;

            return BHState.Success;
        }
    }
}
