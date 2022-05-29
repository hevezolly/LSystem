using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LSystem
{
    public static class XmlParseEssentials
    {
        public const string DefaultNamespace = "LSystem";

        public const string Macros = "macros";
        public const string MacrosName = "name";

        public const string Variables = "variables";
        public const string Variable = "variable";
        public const string VarType = "type";
        public const string VarName = "name";

        public const string Declaration = "declaration";

        public const string Parameters = "parameters";
        public const string Parameter = "parameter";
        public const string ParameterID = "param_id";
        public const string ParameterType = "type";

        public const string Modules = "modules";
        public const string ModuleBase = "base";
        public const string Module = "module";
        public const string ModuleId = "id";

        public const string Axiome = "axiome";

        public const string ContextOptions = "contextOptions";
        public const string IgnoredAsContext = "ignore";
        public const string Branches = "branches";
        public const string BeginBranch = "begin";
        public const string EndBranch = "end";

        public const string Rule = "rule";
        public const string RuleL = "l_rule";
        public const string RuleR = "r_rule";
        public const string RuleLR = "lr_rule";
        public const string LeftContext = "left_context";
        public const string RightContext = "right_context";
        public const string Condition = "condition";
        public const string ParameterUpdate = "parameterUpdate";
        public const string Autowire = "autowire";
        public const string Weight = "weight";
        public const string Priority = "priority";
        public const string RuleSurce = "source";
        public const string RuleSuccessor = "successor";

        public const string SurceVariable = "source";
        public const string LeftContextVariable = "l_context";
        public const string RightContextVariable = "r_context";
        public const string RandomValue = "random";
        public const string TrueRandomValue = "t_random";
        public const string VariablePrefix = "d";

        public const string ParamSurcePattern = "(?<=^|\\W)" + SurceVariable + "\\.([a-zA-Z]\\w*)";
        public const string ParamLeftContextPattern = "(?<=^|\\W)" + LeftContextVariable + "\\.([a-zA-Z]\\w*)";
        public const string ParamRightContextPattern = "(?<=^|\\W)" + RightContextVariable + "\\.([a-zA-Z]\\w*)";
        public const string VariableAccessPattern = "(?<=^|\\W)" + VariablePrefix + "\\.([a-zA-Z]\\w*)";

        public const string RandomPattern = "(?<=^|\\W)" + RandomValue + "(\\d)(?=$|\\W)";
        public const string TrueRandomPattern = "(?<=^|\\W)" + TrueRandomValue + "(?=$|\\W)";

        public const string MacrosUse = "$m$";

        public const string contextName = "context";

        public static void CheckParameterInExpressionString(string expression, string pattern, string surceId,
            Func<string, IModule> getModule,
            string valueNameForError = "surce")
        {
            foreach (Match m in Regex.Matches(expression, pattern))
            {
                var resultParamName = m.Groups[1].Value;
                if (!getModule(surceId).ContainsParam(resultParamName))
                    throw new LSystemCompileException($"rule {valueNameForError} <{surceId}> does not have parameter <{resultParamName}>");
            }
        }

        public static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var builder = new System.Text.StringBuilder();
            var name = type.Name;
            var index = name.IndexOf("`");
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
            builder.Append('<');
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (!first)
                {
                    builder.Append(',');
                }
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }
            builder.Append('>');
            return builder.ToString();
        }

        public static string ApplyReplacement(string expression, string pattern, string surceFieldName, Func<string, IParameter> getParam)
        {
            return Regex.Replace(expression, pattern,
                            (m) => $"{nameof(LSystem)}.{nameof(ModuleExtention)}.{nameof(ModuleExtention.GetParameterValue)}" +
                            $"<{GetFriendlyTypeName(getParam(m.Groups[1].Value).GetValueType())}>({contextName}.{surceFieldName}, \"{m.Groups[1].Value}\")");
        }

        public static string ApplyRandomReplacement(string expression)
        {
            var random = Regex.Replace(expression, RandomPattern,
                           (m) => $"{contextName}.{nameof(RuleExecutionContext.RandomValues)}[{m.Groups[1].Value}]");
            return Regex.Replace(random, TrueRandomPattern,
                (m) => $"((float){contextName}.{nameof(RuleExecutionContext.Random)}.{nameof(System.Random.NextDouble)}())");
        }

        public static string ApplyDataAccessReplacement(string expression, Func<string, string> variableNameToTypeName)
        {
            var result = Regex.Replace(expression, VariableAccessPattern,
                           (m) => $"(({variableNameToTypeName(m.Groups[1].Value)}){VariablePrefix}[\"{m.Groups[1].Value}\"])");
            return result;
        }


        public static string ReplaceExpressionVariables(string expression, 
            bool hasLeftContext, 
            bool hasRightContext, 
            Func<string, IParameter> parameterByIdGetter,
            Func<string, string> variableNameToTypeName)
        {
            var surceReplaced = ApplyReplacement(expression, ParamSurcePattern, nameof(RuleExecutionContext.Source), parameterByIdGetter);
            surceReplaced = ApplyRandomReplacement(surceReplaced);
            surceReplaced = ApplyDataAccessReplacement(surceReplaced, variableNameToTypeName);
            if (hasLeftContext)
            {
                surceReplaced = ApplyReplacement(surceReplaced, ParamLeftContextPattern, nameof(RuleExecutionContext.LeftContext), parameterByIdGetter);
            }
            if (hasRightContext)
            {
                surceReplaced = ApplyReplacement(surceReplaced, ParamRightContextPattern, nameof(RuleExecutionContext.RightContext), parameterByIdGetter);
            }

            return surceReplaced;
        }
    }
}
