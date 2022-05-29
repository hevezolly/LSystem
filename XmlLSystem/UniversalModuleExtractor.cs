using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace LSystem {
    public class UniversalModuleExtractor : IModuleXMLExtractor<UniversalModule>
    {

        public UniversalModule ExtractModule(XElement moduleElem, XMLParseContext<UniversalModule> context)
        {
            var id = moduleElem.Attribute(XmlParseEssentials.ModuleId).Value;
            var parameters = new List<IParameter>();

            var baseElem = moduleElem.Attribute(XmlParseEssentials.ModuleBase);
            if (baseElem != null)
            {
                var baseId = baseElem.Value;
                if (!context.HasModule(baseId))
                    throw new LSystemXmlParseException(moduleElem, $"module with id={baseId} was not defined");
                parameters = context.GetModule(baseId).parameters.Select(p => p.Copy()).ToList();
            }

            var usedParams = new HashSet<string>();

            foreach (var parameterElem in moduleElem.Elements(context.NameSpace + XmlParseEssentials.Parameter))
            {
                var paramId = parameterElem.Attribute(XmlParseEssentials.ParameterID).Value;
                if (usedParams.Contains(paramId))
                    throw new LSystemCompileException($"module <{id}> has multiple parameters <{paramId}>");

                var param = context.GetParameter(paramId).Copy();
                parameters.Add(param);


                usedParams.Add(paramId);
            }

            var module = new UniversalModule(id, parameters);
            return module;
        }
    }
}
