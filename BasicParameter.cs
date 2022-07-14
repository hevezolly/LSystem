using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public class BasicParameter<T> : IParameter<T>
    {
        private string id;

        public BasicParameter(T value, string id)
        {
            Value = value;
            this.id = id;
        }

        public T Value { get; set; }

        public string ParamId => id;

        public IParameter Copy()
        {
            return new BasicParameter<T>(Value, id);
        }

        public void CopyFrom(IParameter<T> other)
        {
            Value = other.Value;
        }

        public bool TryGetValue<T1>(out T1 value)
        {
            value = default(T1);
            var res = Convert.ChangeType(Value, typeof(T1));
            if (res == null)
                return false;
            value = (T1)res;
            return true;
        }

        public override string ToString()
        {
            return $"{id}={Value}";
        }

        public Type GetValueType()
        {
            return typeof(T);
        }
    }
}
