using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LSystem
{

    public class UniversalRuleExtractor : IRuleXMLExtractor<IRule, UniversalModule>
    {

        public IRule ExtractRule(XElement ruleElem,
            XMLParseContext<UniversalModule> context,
            bool hasLeftContext, 
            bool hasRightContext)
        {
            var surceId = ruleElem.Attribute(XmlParseEssentials.RuleSurce).Value;
            var weight = 1f;
            var weightAttr = ruleElem.Attribute(XmlParseEssentials.Weight);
            if (weightAttr != null)
            {
                weight = float.Parse(weightAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            int? priority = null;
            var priorityAttr = ruleElem.Attribute(XmlParseEssentials.Priority);
            if (priorityAttr != null)
                priority = int.Parse(priorityAttr.Value);
            var surce = context.GetModule(surceId);
            UniversalModule leftContext = null;
            UniversalModule rightContext = null;
            if (hasLeftContext)
                leftContext = context.GetModule(ruleElem.Attribute(XmlParseEssentials.LeftContext).Value);
            if (hasRightContext)
                rightContext = context.GetModule(ruleElem.Attribute(XmlParseEssentials.RightContext).Value);

            var result = new List<UniversalModule>();


            var condition = ExtractCondition(ruleElem, hasLeftContext, hasRightContext, context);

            var successorIndex = 0;
            var transformations = new List<IParameterTransformation>();
            foreach (var successorElem in ruleElem.Elements(context.NameSpace + XmlParseEssentials.RuleSuccessor))
            {
                var successorId = successorElem.Attribute(XmlParseEssentials.ModuleId).Value;
                result.Add(context.GetModule(successorId));
                var autowire = false;
                var autowireElem = successorElem.Attribute(XmlParseEssentials.Autowire);
                if (autowireElem != null)
                    autowire = autowireElem.Value == "true";

                var transformation = ExtractParameterTransformations(successorElem,
                    successorIndex, successorId,
                    surceId, leftContext?.Id, rightContext?.Id, autowire, 
                    context);

                transformations.Add(transformation);

                successorIndex++;
            }

            return new UniversalRule(surce, result, transformations, condition, leftContext, rightContext, weight, priority);
        }

        private ICondition ExtractCondition(XElement ruleElem, bool hasLeftContext, bool hasRightContext, XMLParseContext<UniversalModule> context)
        {
            var condElem = ruleElem.Element(context.NameSpace + XmlParseEssentials.Condition);
            if (condElem == null)
                return null;
            var expression = XmlParseEssentials.ReplaceExpressionVariables(condElem.Value, hasLeftContext, hasRightContext, context.GetParameter, context.GetVariableTypeName);
            expression = $"{XmlParseEssentials.contextName} => {{return {expression};}}";
            Task<Func<RuleExecutionContext, bool>> task;
            try
            {
                task = CSharpScript.EvaluateAsync<Func<RuleExecutionContext, bool>>(expression, context.GetScriptOptions(), context.GetExecutionParameters());
            }
            catch (CompilationErrorException e)
            {
                throw new LSystemXmlParseException(condElem, e.Message);
            }
            task.Wait();
            return new LambdaCondition(task.Result);
        }

        private IParameterTransformation ExtractParameterTransformations(XElement successorElem,
            int successorIndex,
            string successorId,
            string surceId,
            string leftContextId,
            string rightContextId,
            bool autowire,
            XMLParseContext<UniversalModule> context)
        {
            var paramUpdateTasks = new List<Task<Action<RuleExecutionContext>>>();
            var notUsedParameters = new HashSet<string>(context.GetModule(successorId).parameters.Select(p => p.ParamId));
            foreach (var paramUpdateElem in successorElem.Elements(context.NameSpace + XmlParseEssentials.ParameterUpdate))
            {
                var paramName = paramUpdateElem.Attribute(XmlParseEssentials.ParameterID).Value;

                if (!context.GetModule(successorId).ContainsParam(paramName))
                    throw new LSystemCompileException($"module <{successorId}> does not contain parameter <{paramName}>");

                notUsedParameters.Remove(paramName);

                var valueType = context.GetParameter(paramName).GetValueType();

                var expression = paramUpdateElem.Value;

                XmlParseEssentials.CheckParameterInExpressionString(expression, XmlParseEssentials.ParamSurcePattern, surceId, context.GetModule, XmlParseEssentials.SurceVariable);
                if (leftContextId != null)
                    XmlParseEssentials.CheckParameterInExpressionString(expression, XmlParseEssentials.ParamLeftContextPattern, leftContextId, context.GetModule, XmlParseEssentials.LeftContextVariable);
                if (rightContextId != null)
                    XmlParseEssentials.CheckParameterInExpressionString(expression, XmlParseEssentials.ParamRightContextPattern, rightContextId, context.GetModule, XmlParseEssentials.RightContextVariable);

                expression = XmlParseEssentials.ReplaceExpressionVariables(expression, leftContextId != null, rightContextId != null, context.GetParameter, context.GetVariableTypeName);
                expression = $"{XmlParseEssentials.contextName} => " +
                    $"{{ {nameof(LSystem)}.{nameof(ModuleExtention)}.{nameof(ModuleExtention.GetParam)}<{XmlParseEssentials.GetFriendlyTypeName(valueType)}>" +
                    $"({XmlParseEssentials.contextName}.{nameof(RuleExecutionContext.Successors)}[{successorIndex}]," +
                    $" \"{paramName}\").Value = {expression}; }}";
                //expression = $"{contextName} => {{ {contextName}.{nameof(RuleExecutionContext.Successors)}[{successorIndex}].{nameof(ModuleExtention.GetParam)}<{valueType}>(\"{paramName}\").Value = {expression}; }}";
                try
                {
                    paramUpdateTasks.Add(CSharpScript.EvaluateAsync<Action<RuleExecutionContext>>(expression, context.GetScriptOptions(), globals: context.GetExecutionParameters()));
                }
                catch (CompilationErrorException e)
                {
                    throw new LSystemXmlParseException(paramUpdateElem, e.Message);
                }
            }
            var tasks = paramUpdateTasks.ToArray();
            var additionActions = new List<Action<RuleExecutionContext>>();
            if (autowire)
            {
                foreach (var unusedParam in notUsedParameters)
                {
                    if (!context.GetModule(surceId).ContainsParam(unusedParam))
                        continue;
                    var contextParam = Expression.Parameter(typeof(RuleExecutionContext));
                    var successorsAccess = Expression.Property(contextParam,
                        typeof(RuleExecutionContext).GetProperty(nameof(RuleExecutionContext.Successors)));
                    var particularSuccessor = Expression.MakeIndex(successorsAccess, typeof(List<IModule>).GetProperty("Item"),
                        new[] { Expression.Constant(successorIndex) });
                    var successorParameter = Expression.Call(particularSuccessor,
                        typeof(IModule).GetMethod(nameof(IModule.GetParam)),
                        Expression.Constant(unusedParam));
                    var parameterType = typeof(IParameter<>).MakeGenericType(context.GetParameter(unusedParam).GetValueType());
                    var typedParam = Expression.Convert(successorParameter, parameterType);
                    var valueToBeAssigned = Expression.Property(typedParam, parameterType.GetProperty("Value"));

                    var surceAccess = Expression.Field(contextParam,
                        typeof(RuleExecutionContext).GetField(nameof(RuleExecutionContext.Source)));
                    var surceParameter = Expression.Call(surceAccess,
                        typeof(IModule).GetMethod(nameof(IModule.GetParam)),
                        Expression.Constant(unusedParam));
                    var typedSurceParam = Expression.Convert(surceParameter, parameterType);
                    var valueToAssign = Expression.Property(typedSurceParam, parameterType.GetProperty("Value"));

                    var assign = Expression.Assign(valueToBeAssigned, valueToAssign);
                    var result = Expression.Lambda<Action<RuleExecutionContext>>(assign, contextParam).Compile();

                    additionActions.Add(result);
                }
            }
            Task.WaitAll(tasks);
            return new LambdaParameterTransformation(tasks.Select(t => t.Result).Concat(additionActions));
        }
    }
}
