using System.Collections;
using System.Collections.Generic;

namespace LSystem {
    public class XmlLSystemProvider : BaseXmlLSystemProvider<UniversalModule, IRule>
    {
        public XmlLSystemProvider(Dictionary<string, object> parametersDict = null, int? seed = null) : base(new UniversalModuleExtractor(), 
            new UniversalRuleExtractor(), parametersDict, seed)
        {
        }
    }
}
