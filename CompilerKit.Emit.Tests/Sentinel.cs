using System;
using System.Reflection;

namespace CompilerKit.Emit
{
    public class Sentinel<T>
    {
        public static readonly Type Type = typeof(Sentinel<T>);
        public static readonly FieldInfo ValueField = typeof(Sentinel<T>).GetField(nameof(Value), BindingFlags.Public | BindingFlags.Instance);
        public static readonly PropertyInfo ValuePropertyProperty = typeof(Sentinel<T>).GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo GetValueMethod = typeof(Sentinel<T>).GetMethod(nameof(GetValue), BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo SetValueMethod = typeof(Sentinel<T>).GetMethod(nameof(SetValue), BindingFlags.Public | BindingFlags.Instance);
        public static readonly ConstructorInfo DefaultCtor = typeof(Sentinel<T>).GetConstructor(Type.EmptyTypes);
        public static readonly ConstructorInfo ValueCtor = typeof(Sentinel<T>).GetConstructor(new[] { typeof(T) });

        public T Value;
        public T ValueProperty
        {
            get { return Value; }
            set { Value = value; }
        }

        public T GetValue() => Value;
        public void SetValue(T newValue) => Value = newValue;

        public Sentinel() { }
        public Sentinel(T value) { Value = value; }
    }
}
