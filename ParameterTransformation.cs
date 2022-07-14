using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public class ParameterTransformation<T> : LambdaParameterTransformation
    {
        /// <summary>
        /// assigns value to particular parameter of particular successor
        /// </summary>
        /// <param name="successorIndex">index of successor to assign to</param>
        /// <param name="parameterName">name of parameter to change</param>
        /// <param name="value">value to be assigned</param>
        public ParameterTransformation(int successorIndex, string parameterName, T value) :
            base(new Action<RuleExecutionContext>[] { (c) => 
            {
                c.Successors[successorIndex].GetParam<T>(parameterName).Value = value;
            } }) 
        { }

        /// <summary>
        /// takes parameter of predecessor transforms it and assigns to the particular successor
        /// </summary>
        /// <param name="successorIndex">index of successor to assign to</param>
        /// <param name="parameterName">name of the parameter. Predecessor and successor shoud both have one</param>
        /// <param name="transformation">transformation of the parameter</param>
        public ParameterTransformation(int successorIndex, string parameterName, Func<T, T> transformation) :
            base(new Action<RuleExecutionContext>[] { (c) =>
            {
                c.Successors[successorIndex].GetParam<T>(parameterName).Value = 
                    transformation(c.Source.GetParameterValue<T>(parameterName));
            } })
        { }

        /// <summary>
        /// assigns value to particular parameter of first successor
        /// </summary>
        /// <param name="parameterName">name of the parameter</param>
        /// <param name="value">value to be assigned</param>
        public ParameterTransformation(string parameterName, T value) :
            base(new Action<RuleExecutionContext>[] { (c) =>
            {
                c.Successors[0].GetParam<T>(parameterName).Value = value;
            } })
        { }

        /// <summary>
        /// takes parameter of predecessor transforms it and assigns to the first successor
        /// </summary>
        /// <param name="parameterName">name of the parameter</param>
        /// <param name="transformation">transformation of the parameter</param>
        public ParameterTransformation(string parameterName, Func<T, T> transformation) :
            base(new Action<RuleExecutionContext>[] { (c) =>
            {
                c.Successors[0].GetParam<T>(parameterName).Value =
                    transformation(c.Source.GetParameterValue<T>(parameterName));
            } })
        { }

        /// <summary>
        /// takes parameter of predecessor, transforms it and assigns to the particular successor
        /// </summary>
        /// <param name="successorIndex">index of successor</param>
        /// <param name="successorParameterName">name of the parameter to be assigned to. Should be typeof(T)</param>
        /// <param name="sourceParameterName">name of the parameter taked from predecessor. Should be typeof(T)</param>
        /// <param name="transformation">transformation of the parameter</param>
        public ParameterTransformation(int successorIndex, string successorParameterName, 
            string sourceParameterName, Func<T, T> transformation) :
            base(new Action<RuleExecutionContext>[] { (c) =>
            {
                c.Successors[successorIndex].GetParam<T>(successorParameterName).Value =
                    transformation(c.Source.GetParameterValue<T>(sourceParameterName));
            } })
        { }
    }


    public class ParameterTransformation<InT, OutT> : LambdaParameterTransformation
    {
        /// <summary>
        /// takes parameter of predecessor, transforms it and assigns to the particular successor
        /// </summary>
        /// <param name="successorIndex">index of successor</param>
        /// <param name="successorParameterName">name of the parameter to be assigned to. Should be typeof(OutT)</param>
        /// <param name="sourceParameterName">name of the parameter taked from predecessor. Should be typeof(InT)</param>
        /// <param name="transformation">transformation of the parameter</param>
        public ParameterTransformation(int successorIndex, string successorParameterName,
            string sourceParameterName, Func<InT, OutT> transformation) :
            base(new Action<RuleExecutionContext>[] { (c) =>
            {
                c.Successors[successorIndex].GetParam<OutT>(successorParameterName).Value =
                    transformation(c.Source.GetParameterValue<InT>(sourceParameterName));
            } })
        { }
    }
}
