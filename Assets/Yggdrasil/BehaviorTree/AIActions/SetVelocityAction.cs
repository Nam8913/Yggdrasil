using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Sets the NPC's Rigidbody2D velocity directly.
    /// Đặt velocity Rigidbody2D của NPC trực tiếp.
    ///
    /// Useful for knockback, dashes, or custom physics movements.
    /// Hữu ích cho knockback, lunge, hoặc các chuyển động vật lý tùy chỉnh.
    /// </summary>
    public class SetVelocityAction : ActionNode
    {
        // Velocity vector to apply (units per second)
        // Vectơ velocity cần áp dụng (đơn vị/giây)
        public Vector2 Velocity { get; set; }

        protected override BHState OnUpdate()
        {
            return Apply();
        }

        protected override BHState OnExecute()
        {
            return Apply();
        }

        private BHState Apply()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return BHState.Failure;

            var rb = self.GetComponent<Rigidbody2D>();
            if (rb == null)
                return BHState.Failure;

            rb.linearVelocity = Velocity;
            return BHState.Success;
        }
    }
}
