using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    class LambdaCondition : ICondition
    {
        private Func<RuleExecutionContext, bool> lambda;

        public LambdaCondition(Func<RuleExecutionContext, bool> lambda)
        {
            this.lambda = lambda;
        }

        public bool isConditionMet(RuleExecutionContext rule)
        {
            return lambda(rule);
        }
    }
}
