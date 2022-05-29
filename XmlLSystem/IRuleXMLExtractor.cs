using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LSystem
{
    public interface IRuleXMLExtractor<RuleType, ModuleType>
        where RuleType: IRule
        where ModuleType: IModule
    {
        RuleType ExtractRule(XElement baseElement, 
            XMLParseContext<ModuleType> context,
            bool hasLeftContext,
            bool hasRightContext);
    }
}
