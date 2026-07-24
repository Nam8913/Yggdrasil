using System;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Custom condition that checks the NPC's GameObject with a lambda.
    /// Điều kiện tùy chỉnh kiểm tra GameObject của NPC bằng lambda.
    ///
    /// Usage: ".Condition(new GOCondition().Set(go => go.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 0))"
    /// Sử dụng: kiểm tra NPC có đang di chuyển không.
    /// </summary>
    public class GOCondition : ConditionNode
    {
        // Lambda function that receives the GameObject and returns a bool
        // Lambda function nhận GameObject và trả về bool
        public Func<GameObject, bool> Func { get; set; }

        protected override bool Check()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            if (Func == null)
            {
                UnityEngine.Debug.LogWarning("[GOCondition] Func is null.");
                return false;
            }

            return Func(self);
        }

        public GOCondition Set(Func<GameObject, bool> func)
        {
            Func = func;
            return this;
        }
    }
}
