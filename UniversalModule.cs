using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSystem
{
    public class UniversalModule : IModule
    {
        public string Id => _symb;
        private string _symb;

        private Dictionary<string, IParameter> _parameters;
        public IEnumerable<IParameter> parameters => _parameters.Values;

        public List<IModule> Successors => successors;


        private List<IModule> successors = new List<IModule>();

        public UniversalModule(string symb, IEnumerable<IParameter> parameters)
        {
            _symb = symb;
            _parameters = new Dictionary<string, IParameter>();
            foreach (var p in parameters)
            {
                _parameters[p.ParamId] = p;
            }
        }

        public UniversalModule(string symb) : this(symb, new IParameter[0]) { }

        public IParameter GetParam(string paramId)
        {
            if (!_parameters.ContainsKey(paramId))
                return null;
            return _parameters[paramId];
        }

        public override string ToString()
        {
            if (parameters == null || _parameters.Count == 0)
                return Id;
            return $"{Id}({string.Join(", ", parameters.Select(v => v.ToString()))})";
        }

        public bool ContainsParam(string paramId)
        {
            return _parameters.ContainsKey(paramId);
        }

        public Type GetParameterType(string paramId)
        {
            return _parameters[paramId].GetValueType();
        }

        public IModule Copy()
        {
            var newParameters = new List<IParameter>();
            foreach (var p in parameters)
            {
                newParameters.Add(p.Copy());
            }
            return new UniversalModule(Id, newParameters);
        }
    }
}
