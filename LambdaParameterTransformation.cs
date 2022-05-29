using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    class LambdaParameterTransformation : IParameterTransformation
    {
        private IEnumerable<Action<RuleExecutionContext>> transformations;

        public LambdaParameterTransformation(IEnumerable<Action<RuleExecutionContext>> transformations)
        {
            this.transformations = transformations;
        }

        public void ApplyTo(RuleExecutionContext ruleContext)
        {
            foreach (var t in transformations)
            {
                t(ruleContext);
            }
        }
    }
}
