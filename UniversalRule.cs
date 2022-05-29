using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSystem
{
    public class UniversalRule: IRule
    {
        public UniversalRule(IModule source, IEnumerable<IModule> successors, IEnumerable<IParameterTransformation> transformations = null, 
            ICondition condition = null, IModule leftContext = null, IModule rightContext = null, float weight=1, int? fixedPriority = null)
        {
            this.surce = source;
            this.Successors = successors.ToList();
            this.condition = condition;
            this.leftContext = leftContext;
            this.rightContext = rightContext;
            this.Weight = weight;
            this.transformations = new List<IParameterTransformation>();
            if (transformations != null)
                this.transformations = new List<IParameterTransformation>(transformations);
            Priority = CalculatePriority(fixedPriority);
        }

        private int CalculatePriority(int? fixedProirity)
        {
            if (fixedProirity != null)
                return fixedProirity.Value;
            var conditionValue = (condition != null) ? 2 : 0;
            var leftContextValue = (leftContext != null) ? 4 : 0;
            var rightContextValue = (rightContext != null) ? 4 : 0;

            return conditionValue + leftContextValue + rightContextValue;
        }

        public float Weight { get; private set; }

        public int Priority { get; private set; }

        private IModule leftContext;
        private IModule rightContext;

        private IModule surce;

        public IModule Source => surce;

        public IModule LeftContext => leftContext;

        public IModule RightContext => rightContext;

        public IEnumerable<IModule> Successors { get; private set; }

        private ICondition condition;

        private IEnumerable<IParameterTransformation> transformations;

        public IEnumerable<IModule> Apply(RuleExecutionContext executionContext)
        {
            if (executionContext == null)
                return new List<IModule>();
            foreach (var transform in transformations)
            {
                transform.ApplyTo(executionContext);
            }
            return executionContext.Successors;
        }

        public bool CanBeApplied(RuleExecutionContext context)
        {
            if (context == null)
                return false;
            if (context.Source.Id != surce.Id)
                return false;
            if (context.LeftContext?.Id != leftContext?.Id)
                return false;
            if (context.RightContext?.Id != rightContext?.Id)
                return false;
            return (condition == null || condition.isConditionMet(context));
        }
    }
}
