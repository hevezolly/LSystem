using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSystem
{
    public interface IModule
    {
        string Id { get; }

        List<IModule> Successors { get; }
        IEnumerable<IParameter> parameters { get; }

        IParameter GetParam(string id);

        bool ContainsParam(string paramId);

        Type GetParameterType(string paramId);

        IModule Copy();
    }

    public static class ModuleExtention
    {

        public static T GetParameterValue<T>(this IModule module, string paramId, T defaultValue = default(T))
        {
            var param = module.GetParam(paramId);
            if (param == null)
                return defaultValue;
            if (param.TryGetValue<T>(out var result))
                return result;
            return defaultValue;
        }


        public static IParameter<T> GetParam<T>(this IModule module, string id)
        {
            var param = module.GetParam(id);
            return param as IParameter<T>;
        }
    }
}
