namespace BehaviorTree
{
    /// <summary>
    /// XOR gate: succeeds when children return a MIX of Success and Failure.
    /// Cổng XOR: thành công khi các con trả về CẢ Success lẫn Failure.
    ///
    /// - BreakOnMixed = true (default): return Success as soon as both success and failure are found.
    /// - BreakOnMixed = false: tick all children first, then check.
    /// BreakOnMixed = true (mặc định): trả về Success ngay khi tìm thấy cả success và failure.
    /// BreakOnMixed = false: tick tất cả con trước, rồi kiểm tra.
    /// Truth table (2 children):
    /// Bảng chân trị (2 con):
    ///   A=Success, B=Success → Failure  (all same = no disagreement)
    ///   A=Success, B=Failure → Success  (mixed = disagreement)
    ///   A=Failure, B=Success → Success  (mixed = disagreement)
    ///   A=Failure, B=Failure → Failure  (all same = no disagreement)
    ///
    /// Usage: "detect conflicting states" / "check if behaviors disagree".
    /// Sử dụng: "phát hiện trạng thái mâu thuẫn" / "kiểm tra nếu hành vi bất đồng".
    /// </summary>
    public class XorNode : CompositeNode
    {
        // If true, return Success immediately when mismatch (success + failure) is found
        // Nếu true, trả về Success ngay khi tìm thấy mâu thuẫn (success + failure)
        public bool BreakOnMixed { get; set; } = true;

        protected override BHState OnUpdate()
        {
            bool hasSuccess = false;
            bool hasFailure = false;

            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                    hasSuccess = true;

                if (state == BHState.Failure)
                    hasFailure = true;

                CurrentChildIndex++;

                if (BreakOnMixed && hasSuccess && hasFailure)
                    break;
            }

            CurrentChildIndex = 0;
            return (hasSuccess && hasFailure) ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            bool hasSuccess = false;
            bool hasFailure = false;

            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                    hasSuccess = true;

                if (state == BHState.Failure)
                    hasFailure = true;

                CurrentChildIndex++;

                if (BreakOnMixed && hasSuccess && hasFailure)
                    break;
            }

            CurrentChildIndex = 0;
            return (hasSuccess && hasFailure) ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Failure;
        }
    }
}
