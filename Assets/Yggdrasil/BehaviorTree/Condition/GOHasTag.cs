using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Checks if the NPC's GameObject has one of the specified tags.
    /// Kiểm tra xem GameObject của NPC có một trong các tag được chỉ định hay không.
    ///
    /// Usage: ".Condition(new GOHasTag().SetTags("Enemy", "Boss"))"
    /// Sử dụng: kiểm tra NPC có tag "Enemy" hoặc "Boss" không.
    /// </summary>
    public class GOHasTag : ConditionNode
    {
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        protected override bool Check()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            foreach (var tag in Tags)
            {
                if (self.CompareTag(tag))
                    return true;
            }
            return false;
        }

        public GOHasTag SetTags(List<string> tags)
        {
            this.Tags = new HashSet<string>(tags);
            return this;
        }

        public GOHasTag SetTags(params string[] tags)
        {
            this.Tags = new HashSet<string>(tags);
            return this;
        }

        public GOHasTag AddTag(string tag)
        {
            Tags.Add(tag);
            return this;
        }
    }
}