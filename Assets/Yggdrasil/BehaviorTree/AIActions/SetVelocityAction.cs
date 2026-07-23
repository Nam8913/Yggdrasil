using UnityEngine;

namespace BehaviorTree
{
    public class SetVelocityAction : ActionNode
    {
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
