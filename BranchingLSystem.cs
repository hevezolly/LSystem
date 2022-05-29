using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LSystem
{
    class BranchingLSystem<ModuleType, RuleType>: ContextFilteringLSystem<ModuleType, RuleType>
        where ModuleType : class, IModule
        where RuleType : class, IRule
    {
        private string branchStart;
        private string branchEnd;
        public BranchingLSystem(IEnumerable<ModuleType> axiome, 
            IEnumerable<ModuleType> alphabet,
            IEnumerable<RuleType> rules,
            string branchStartId,
            string branchEndId,
            IEnumerable<string> contextIgnoredIds = null,
            int? seed = null) : 
            base(axiome, 
                alphabet, 
                rules, 
                ((contextIgnoredIds == null) ? new HashSet<string>() : contextIgnoredIds)
                .Append(branchStartId)
                .Append(branchEndId),
                seed)
        {
            branchStart = branchStartId;
            branchEnd = branchEndId;
        }

        protected override ModuleType GetRightContext(int index, string fittingId)
        {
            var variants = GetPossibleRightContext(index);
            return variants.FirstOrDefault(m => m.Id == fittingId);
        }

        private HashSet<ModuleType> GetPossibleRightContext(int index)
        {
            if (index >= CurrentState.Count() - 1)
                return new HashSet<ModuleType>();
            var passedBranchSymbols = 0;
            var result = new HashSet<ModuleType>();
            var needToAdd = true;
            foreach (var m in CurrentState.Skip(index + 1))
            {
                var current = m.Id;
                if (current == branchStart)
                {
                    passedBranchSymbols++;
                }
                else if (current == branchEnd)
                {
                    if (passedBranchSymbols == 0)
                        return result;
                    passedBranchSymbols--;
                    if (passedBranchSymbols == 0)
                        needToAdd = true;
                }
                else if (ignoresIds.Contains(current))
                {
                    continue;
                }
                else if (needToAdd == true)
                {
                    result.Add(m);
                    needToAdd = false;
                    if (passedBranchSymbols == 0)
                        return result;
                }
            }
            if (passedBranchSymbols == 0)
                return result;
            throw new LSystemExecutionException($"l-system produces incorrect bracket sequence with modules <{branchStart}> and <{branchEnd}>");
        }

        protected override ModuleType GetLeftContext(int index)
        {
            if (index <= 0)
                return null;

            var passedBranchSymbols = 0;

            foreach (var m in CurrentState.Take(index).Reverse())
            {
                var current = m.Id;
                if (current == branchEnd)
                {
                    passedBranchSymbols++;
                }
                else if (current == branchStart)
                {
                    if (passedBranchSymbols > 0)
                        passedBranchSymbols--;
                }
                else if (ignoresIds.Contains(current))
                {
                    continue;
                }
                else if (passedBranchSymbols == 0)
                {
                    return m;
                }
            }
            if (passedBranchSymbols == 0)
                return null;
            throw new LSystemExecutionException($"l-system produces incorrect bracket sequence with modules <{branchStart}> and <{branchEnd}>");
        }
    }
}
