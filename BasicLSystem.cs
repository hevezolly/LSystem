using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public class BasicLSystem<ModuleType, RuleType> : ILSystem<ModuleType>
        where ModuleType: class, IModule
        where RuleType: class, IRule
    {

        private List<ModuleType> currentState;

        public IEnumerable<ModuleType> CurrentState => currentState;

        IEnumerable<IModule> ILSystem.CurrentState => currentState;

        private Dictionary<string, List<RuleType>> hashedRules;

        private List<ModuleType> axiome;

        private List<ModuleType> alphabet;

        private List<RuleType> rules;

        private Random random;

        public BasicLSystem(IEnumerable<ModuleType> axiome, IEnumerable<ModuleType> alphabet, IEnumerable<RuleType> rules, int? seed = null)
        {
            this.axiome = axiome.ToList();
            this.alphabet = alphabet.ToList();
            this.rules = rules.ToList();
            random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void Init()
        {
            currentState = axiome.Select(l => (ModuleType)l.Copy()).ToList();
            hashedRules = new Dictionary<string, List<RuleType>>();
            foreach (var symb in alphabet.Select(m => m.Id))
            {
                hashedRules[symb] = rules.Where(r => r.Source.Id == symb).ToList();
            }
        }

        private RuleType SelectOneRule(IEnumerable<RuleType> allRules)
        {
            if (allRules.Count() == 0)
                return null;

            var maxPriority = allRules.Max(r => r.Priority);
            var rules = allRules.Where(r => r.Priority == maxPriority);
            if (rules.Count() == 1)
                return rules.First();
            var len = rules.Sum(r => r.Weight);
            var value = random.NextDouble() * len;
            var cumulated = 0f;
            foreach (var rule in rules)
            {
                if (value >= cumulated && value < cumulated + rule.Weight)
                    return rule;
                cumulated += rule.Weight;
            }
            return null;
        }

        private RuleExecutionContext FormExecutionContext(int index, RuleType rule)
        {
            var seed = random.Next(0, int.MaxValue);
            var source = currentState[index];
            ModuleType leftContext = null;
            ModuleType rightContext = null;
            if (rule.LeftContext != null)
                leftContext = GetLeftContext(index);
            if (rule.RightContext != null)
                rightContext = GetRightContext(index, rule.RightContext.Id);
            return new RuleExecutionContext(source, seed, leftContext, rightContext);
        }

        public void Step()
        {
            var result = new List<ModuleType>();
            for (var i = 0; i < currentState.Count; i++)
            {
                var possibleRulesToContext = new Dictionary<RuleType, RuleExecutionContext>();
                foreach (var r in hashedRules[currentState[i].Id])
                {
                    var context = FormExecutionContext(i, r);
                    if (!r.CanBeApplied(context))
                        continue;
                    context.SetSuccessors(r.Successors.Select(s => (ModuleType)s.Copy()));
                    possibleRulesToContext[r] = context;
                }
                var chousen = SelectOneRule(possibleRulesToContext.Keys);
                if (chousen == null)
                    currentState[i].Successors.Add((ModuleType)currentState[i].Copy());
                else
                    currentState[i].Successors.AddRange(chousen.Apply(possibleRulesToContext[chousen]));
                result.AddRange(currentState[i].Successors.Select(m => (ModuleType)m));
            }
            currentState = result;
        }

        protected virtual ModuleType GetRightContext(int index, string fittingId)
        {
            if (index >= currentState.Count - 1)
                return null;
            return currentState[index + 1];
        }

        protected virtual ModuleType GetLeftContext(int index)
        {
            if (index <= 0)
                return null;
            return currentState[index - 1];
        }
    }
}
