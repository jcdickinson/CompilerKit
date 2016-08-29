using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CompilerKit.Emit.Ssa
{
    public class CallInstruction : Instruction
    {
        public Variable Instance { get; }
        public MethodInfo Method { get; }
        public Variable Output { get; }
        public IReadOnlyList<Variable> Parameters { get; }

        public override IReadOnlyList<Variable> InputVariables { get; }
        public override IReadOnlyList<Variable> OutputVariables { get; }

        private readonly ParameterInfo[] _parameters;

        public CallInstruction(Variable instance, MethodInfo method, Variable output, IEnumerable<Variable> parameters)
            : this(instance, method, output, (parameters as IList<Variable>) ?? parameters.ToList())
        {

        }

        public CallInstruction(Variable instance, MethodInfo method, Variable output, params Variable[] parameters)
            : this(instance, method, output, (IList<Variable>)parameters)
        {

        }

        public CallInstruction(Variable instance, MethodInfo method, Variable output, IList<Variable> parameters)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if ((instance == null) != method.IsStatic) throw new ArgumentOutOfRangeException(nameof(instance));
            if (method.ReturnType == typeof(void) && output != null) throw new ArgumentOutOfRangeException(nameof(output));
            if (instance != null && !instance.Type.IsAssignableFrom(method.DeclaringType)) throw new ArgumentOutOfRangeException(nameof(instance));

            Instance = instance;
            Method = method;
            Output = output;
            Parameters = new ReadOnlyCollection<Variable>(parameters);

            _parameters = method.GetParameters();
            if (parameters.Count != _parameters.Length) throw new ArgumentOutOfRangeException(nameof(parameters));

            var inputCount = 0;
            var outputCount = 0;

            if (instance != null) inputCount++;
            if (output != null) outputCount++;

            for (var i = 0; i < parameters.Count; i++)
            {
                if (_parameters[i].IsIn) inputCount++;
                if (_parameters[i].IsOut) outputCount++;
            }

            var inputs = new Variable[inputCount];
            var outputs = new Variable[outputCount];

            inputCount = 0;
            outputCount = 0;

            if (instance != null) inputs[inputCount++] = instance;
            if (output != null) outputs[inputCount++] = output;

            for (var i = 0; i < parameters.Count; i++)
            {
                if (_parameters[i].IsIn) inputs[inputCount++] = parameters[i];
                if (_parameters[i].IsOut) outputs[outputCount++] = parameters[i];
            }

            InputVariables = new ReadOnlyCollection<Variable>(inputs);
            OutputVariables = new ReadOnlyCollection<Variable>(outputs);
        }

        public override void CompileTo(IILGenerator il)
        {
            if (!Method.IsStatic)
                il.Load(Instance, EmitOptions.None);

            for (var i = 0; i < _parameters.Length; i++)
            {
                il.Load(Parameters[i], EmitOptions.None);
                if (_parameters[i].IsOut) throw new NotSupportedException();
            }

            il.Call(Method);

            if (Method.ReturnType != typeof(void))
                il.Store(Output, EmitOptions.None);
        }
    }
}
