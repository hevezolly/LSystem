using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public interface IParameterTransformation
    {
        void ApplyTo(RuleExecutionContext ruleContext);
    }
}
