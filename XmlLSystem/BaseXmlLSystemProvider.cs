using System;
using System.Dynamic;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace LSystem
{
    public class BaseXmlLSystemProvider<ModuleType, RuleType> : ILSystemProvider<ModuleType>
        where ModuleType : class, IModule
        where RuleType : class, IRule
    {
        private XDocument parsedLSML;

        private XNamespace ns;

        private object parameters;

        private Dictionary<string, object> parametersDict;

        private Dictionary<string, IParameter> idToParam = new Dictionary<string, IParameter>();
        private Dictionary<string, ModuleType> idToModule = new Dictionary<string, ModuleType>();

        private ScriptOptions options;

        private Dictionary<string, string> variableNameToType;

        private int? seed;

        private IModuleXMLExtractor<ModuleType> moduleExtractor;
        private IRuleXMLExtractor<RuleType, ModuleType> ruleExtractor;

        public BaseXmlLSystemProvider(IModuleXMLExtractor<ModuleType> moduleExtractor, IRuleXMLExtractor<RuleType, ModuleType> ruleExtractor, Dictionary<string, object> parametersDict, int? seed = null)
        {
            ns = XmlParseEssentials.DefaultNamespace;
            this.parametersDict = parametersDict;
            this.seed = seed;
            this.moduleExtractor = moduleExtractor;
            this.ruleExtractor = ruleExtractor;
        }

        private XMLParseContext<ModuleType> GetContext()
        {
            return new XMLParseContext<ModuleType>(idToModule, idToParam, variableNameToType, ns, parameters, GetOptions());
        }

        private ParameterObject CreateParameters()
        {
            if (parametersDict == null)
                return null;
            return new ParameterObject(parametersDict);
        }

        public BaseXmlLSystemProvider<ModuleType, RuleType> Load(string filePath)
        {
            SetDocument(XDocument.Load(filePath));
            return this;
        }

        public BaseXmlLSystemProvider<ModuleType, RuleType> Parse(string xml)
        {
            SetDocument(XDocument.Parse(xml));
            return this;
        }

        private void SetDocument(XDocument document)
        {
            
            parsedLSML = ReplaceMacrosEntry(document);
            CheckVariables();
            parameters = CreateParameters();
        }

        private XDocument ReplaceMacrosEntry(XDocument document)
        {
            foreach (var macrosElement in document.Descendants().Where(e => e.Name == ns + XmlParseEssentials.Macros))
            {
                
                var parent = macrosElement.Parent;
                
                var macrosPattern = XmlParseEssentials.MacrosUse.Replace("m", macrosElement.Attribute(XmlParseEssentials.MacrosName).Value);
                var replacement = macrosElement.Value;
                foreach (var element in parent.Descendants().Where(e => e.Name.LocalName != XmlParseEssentials.Macros).Where(e => !e.HasElements))
                {
                    element.Value = element.Value.Replace(macrosPattern, replacement);
                }
            }
            return document;
        }

        private void CheckVariables()
        {
            variableNameToType = new Dictionary<string, string>();
            var names = new List<string>();
            var types = new List<string>();
            var expression = new StringBuilder();
            if (parsedLSML.Root.Element(ns + XmlParseEssentials.Variables) == null)
                return;
            foreach (var varElem in parsedLSML.Root.Element(ns + XmlParseEssentials.Variables)?.Elements(ns + XmlParseEssentials.Variable))
            {
                var typeStr = varElem.Attribute(XmlParseEssentials.VarType).Value;
                var name = varElem.Attribute(XmlParseEssentials.VarName).Value;
                expression.AppendLine($"var t{names.Count} = typeof({typeStr});");
                names.Add(name);
                types.Add(typeStr);
            }

            if (parametersDict == null)
                throw new LSystemCompileException($"parameters expected but none where found");
            Task<ScriptState<object>> t;
            try
            {
                t = CSharpScript.RunAsync(expression.ToString(), GetOptions());
            }
            catch (CompilationErrorException e)
            {
                throw new LSystemXmlParseException(parsedLSML.Root.Element(ns + XmlParseEssentials.Variables), e.Message);
            }
            t.Wait();
            foreach (var typeVar in t.Result.Variables)
            {
                var index = int.Parse(typeVar.Name.Substring(1));
                var type = (Type)(typeVar.Value);

                var name = names[index];

                if (!parametersDict.ContainsKey(name))
                {
                    throw new LSystemCompileException($"xml parser parameters should contain entry {name} with type {types[index]}");
                }
                else if (parametersDict[name].GetType() != type)
                {
                    throw new LSystemCompileException($"parameter {name} should be {types[index]}, not {parametersDict[name].GetType()}");
                }
                variableNameToType[name] = types[index];
            }
        }

        private ScriptOptions GetOptions()
        {
            return ScriptOptions.Default
                .AddImports(nameof(System), nameof(LSystem))//, nameof(UnityEngine))
                .AddReferences(typeof(UniversalRule).Assembly);//, typeof(UnityEngine.GameObject).Assembly);
        }


        private IEnumerable<RuleType> ExtractRules()
        {
            var rules = new List<RuleType>();
            var rulesElements = parsedLSML.Root.Elements(ns + XmlParseEssentials.Rule).Select(re => new { element = re, hasLeft = false, hasRight = false })
                .Concat(parsedLSML.Root.Elements(ns + XmlParseEssentials.RuleL).Select(re => new { element = re, hasLeft = true, hasRight = false }))
                .Concat(parsedLSML.Root.Elements(ns + XmlParseEssentials.RuleR).Select(re => new { element = re, hasLeft = false, hasRight = true }))
                .Concat(parsedLSML.Root.Elements(ns + XmlParseEssentials.RuleLR).Select(re => new { element = re, hasLeft = true, hasRight = true }));
            foreach (var ruleParams in rulesElements)
            {
                rules.Add(ruleExtractor.ExtractRule(ruleParams.element, GetContext(), ruleParams.hasLeft, ruleParams.hasRight));
            }
            return rules;
        }

        private IEnumerable<ModuleType> ExtractAxiome()
        {
            var result = new List<ModuleType>();
            
            foreach (var axiomElem in parsedLSML.Root.Element(ns + XmlParseEssentials.Declaration).Element(ns + XmlParseEssentials.Axiome).Elements(ns + XmlParseEssentials.Module))
            {
                var axiomeId = axiomElem.Attribute(XmlParseEssentials.ModuleId).Value;
                
                result.Add(idToModule[axiomeId]);
            }
            
            return result;
        }

        private IEnumerable<ModuleType> ExtractModules(Dictionary<string, IParameter> paramIdToParam)
        {
            foreach (var moduleElem in parsedLSML.Root.Element(ns + XmlParseEssentials.Declaration).Element(ns + XmlParseEssentials.Modules).Elements(ns + XmlParseEssentials.Module))
            {
                yield return moduleExtractor.ExtractModule(moduleElem, GetContext());
            }
        }

        private IEnumerable<IParameter> ExtractParameters()
        {
            
            var idToTask = new Dictionary<Task<ScriptState<object>>, string>();
            foreach (var parameterElem in parsedLSML.Root.Element(ns + XmlParseEssentials.Declaration)
                .Element(ns + XmlParseEssentials.Parameters).Elements(ns + XmlParseEssentials.Parameter))
            {
                var typeStr = parameterElem.Attribute(XmlParseEssentials.ParameterType).Value;
                var paramId = parameterElem.Attribute(XmlParseEssentials.ParameterID).Value;

                var expression = XmlParseEssentials.ApplyDataAccessReplacement(parameterElem.Value, (v) => variableNameToType[v]);
                try
                {
                    idToTask[CSharpScript.RunAsync($"var type=typeof({typeStr}); var value={expression};",GetOptions(), globals: parameters)] = paramId;
                }
                catch (CompilationErrorException e)
                {
                    throw new LSystemXmlParseException(parameterElem, e.Message);
                }
            }

            var tasks = idToTask.Keys.ToArray();
            Task.WaitAll(tasks);

            foreach (var res in tasks)
            {
                var valueType = (Type)res.Result.GetVariable("type").Value;
                object parameterValue;
                try
                {
                    parameterValue = Convert.ChangeType(res.Result.GetVariable("value").Value, valueType);
                }
                catch(InvalidCastException)
                {
                    throw new LSystemCompileException($"can't convert {res.Result.GetVariable("value").Value} to type {valueType}");
                }

                var paramId = idToTask[res];

                var paramType = typeof(BasicParameter<>).MakeGenericType(valueType);
                var param = (IParameter)paramType.GetConstructor(new[] { valueType, typeof(string) }).Invoke(new[] { parameterValue, paramId });
                
                yield return param;
            }
        }

        private struct BranchSymbols
        {
            public string begin;
            public string end;
        }

        private BranchSymbols? ExtractBranches()
        {
            var options = parsedLSML.Root.Element(ns + XmlParseEssentials.Declaration).Element(ns + XmlParseEssentials.ContextOptions);
            if (options == null)
                return null;
            var branch = options.Element(ns + XmlParseEssentials.Branches);
            if (branch == null)
                return null;
            return new BranchSymbols()
            {
                begin = branch.Element(ns + XmlParseEssentials.BeginBranch).Attribute(XmlParseEssentials.ModuleId).Value,
                end = branch.Element(ns + XmlParseEssentials.EndBranch).Attribute(XmlParseEssentials.ModuleId).Value
            };
        }

        private HashSet<string> ExtractIgnoredContext()
        {
            var options = parsedLSML.Root.Element(ns + XmlParseEssentials.Declaration).Element(ns + XmlParseEssentials.ContextOptions);
            if (options == null)
                return null;
            var ignore = options.Element(ns + XmlParseEssentials.IgnoredAsContext);
            if (ignore == null)
                return null;
            return new HashSet<string>(ignore.Elements(ns + XmlParseEssentials.Module).Select(e => e.Attribute(XmlParseEssentials.ModuleId).Value));
        }

        ILSystem ILSystemProvider.Compile()
        {
            return Compile();
        }

        public ILSystem<ModuleType> Compile()
        {
            idToModule = new Dictionary<string, ModuleType>();
            idToParam = new Dictionary<string, IParameter>();

            foreach (var p in ExtractParameters())
            {
                idToParam[p.ParamId] = p;
            }

            foreach (var m in ExtractModules(idToParam))
            {
                idToModule[m.Id] = m;
            }


            var axiome = ExtractAxiome();



            var rules = ExtractRules();


            var branches = ExtractBranches();
            var ignores = ExtractIgnoredContext();

            if (branches != null)
                return new BranchingLSystem<ModuleType, RuleType>(axiome, idToModule.Values, rules, branches.Value.begin, branches.Value.end, ignores, seed);

            if (ignores != null)
                return new ContextFilteringLSystem<ModuleType, RuleType>(axiome, idToModule.Values, rules, ignores, seed);

            return new BasicLSystem<ModuleType, RuleType>(axiome, idToModule.Values, rules, seed);
        }
    }

    public class ParameterObject
    {
        public Dictionary<string, object> d;
        public ParameterObject(Dictionary<string, object> data)
        {
            d = data;
        }
    }
}
