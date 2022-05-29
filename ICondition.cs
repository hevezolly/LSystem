using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public interface ICondition
    {
        bool isConditionMet(RuleExecutionContext rule);
    }
}
