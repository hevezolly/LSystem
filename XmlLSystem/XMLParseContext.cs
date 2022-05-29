using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LSystem {
    public class XMLParseContext<ModuleType>
        where ModuleType : IModule
    {
        private Dictionary<string, ModuleType> idToModule;
        private Dictionary<string, IParameter> possibleParameters;
        private Dictionary<string, string> variableNameToType;
        private XNamespace ns;
        private ScriptOptions options;
        private object parameters;

        public XMLParseContext(Dictionary<string, ModuleType> idToModule,
            Dictionary<string, IParameter> possibleParameters,
            Dictionary<string, string> varNameToType,
            XNamespace ns, object parameters, ScriptOptions options = null)
        {
            this.idToModule = idToModule;
            this.possibleParameters = possibleParameters;
            this.ns = ns;
            this.options = options;
            variableNameToType = varNameToType;
            this.parameters = parameters;
        }

        public ScriptOptions GetScriptOptions()
        {
            if (options != null)
                return options;
            return ScriptOptions.Default
                .AddImports(nameof(System), nameof(LSystem), nameof(UnityEngine))
                .AddReferences(typeof(UniversalRule).Assembly, typeof(UnityEngine.GameObject).Assembly);
        }

        public object GetExecutionParameters() => parameters;

        public ModuleType GetModule(string id) => idToModule[id];

        public IParameter GetParameter(string id) => possibleParameters[id];

        public bool HasModule(string id) => idToModule.ContainsKey(id);

        public bool HasParameter(string id) => possibleParameters.ContainsKey(id);

        public string GetVariableTypeName(string variableName) => variableNameToType[variableName];

        public XNamespace NameSpace => ns;
    }
}
