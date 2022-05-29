using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public interface IRule
    {
        bool CanBeApplied(RuleExecutionContext context);

        IEnumerable<IModule> Apply(RuleExecutionContext context);

        IModule Source { get; }

        IModule LeftContext { get; }

        IModule RightContext { get; }

        IEnumerable<IModule> Successors { get; }

        float Weight { get; }

        int Priority { get; }
    }
}
