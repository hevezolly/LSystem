using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LSystem
{
    class ContextFilteringLSystem<ModuleType, RuleType> : BasicLSystem<ModuleType, RuleType>
        where ModuleType : class, IModule
        where RuleType : class, IRule
    {
        protected HashSet<string> ignoresIds;
        public ContextFilteringLSystem(IEnumerable<ModuleType> axiome, 
            IEnumerable<ModuleType> alphabet, 
            IEnumerable<RuleType> rules,
            IEnumerable<string> contextIgnoredIds,
            int? seed = null) : 
            base(axiome, alphabet, rules, seed)
        {
            ignoresIds = new HashSet<string>(contextIgnoredIds);
        }

        protected override ModuleType GetRightContext(int index, string fittingId)
        {
            foreach (var m in CurrentState.Skip(index+1))
            {
                if (ignoresIds.Contains(m.Id))
                    continue;
                return m;
            }
            return null;
        }

        protected override ModuleType GetLeftContext(int index)
        {
            foreach (var m in CurrentState.Take(index).Reverse())
            {
                if (ignoresIds.Contains(m.Id))
                    continue;
                return m;
            }
            return null;
        }
    }
}
