using System;

namespace LSystem
{
    public interface IParameter
    {
        string ParamId { get; }

        IParameter Copy();

        bool TryGetValue<T>(out T value);

        Type GetValueType();
    }

    public interface IParameter<T> : IParameter
    {
        T Value { get; set; }

        void CopyFrom(IParameter<T> other);
    }
}