using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LSystem
{
    public interface IModuleXMLExtractor<ModuleType>
        where ModuleType : IModule
    {
        ModuleType ExtractModule(XElement baseElement, XMLParseContext<ModuleType> context);
    }
}
