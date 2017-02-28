using System.Collections.Generic;
using CompilerKit.Emit.Ssa.Services;
using CompilerKit.Collections.Generic;

namespace CompilerKit.Emit.Ssa.Optimizers
{
    /// <summary>
    /// Represents an optimizer that performs variable destruction.
    /// </summary>
    public sealed class VariableDestructionOptimizer : IOptimizer
    {
        public static VariableDestructionOptimizer Instance { get; } = new VariableDestructionOptimizer();

        internal sealed class DisjointSetVariableService : IVariableService
        {
            IEnumerable<Variable> IVariableService.Parameters => Parameters;

            IEnumerable<Variable> IVariableService.Locals => Locals;

            public List<Variable> Parameters { get; }
            public DisjointSet<Variable> Locals { get; }

            internal DisjointSetVariableService()
            {
                Parameters = new List<Variable>();
                Locals = new DisjointSet<Variable>();
            }

            public Variable FindVariable(Variable variable) => variable.IsParameter
                ? variable
                : Locals[variable];

            public bool Free()
            {
                Parameters.Clear();
                Locals.Clear();
                return SsaFactory.Pools.DisjointSetVariableService.Free(this);
            }
        }

        /// <summary>
        /// Optimizes the specified target.
        /// </summary>
        /// <param name="target">The target to optimize.</param>
        public void Optimize(IBodyTarget target)
        {
            var oldService = target.GetService<IVariableService>();
            var newService = SsaFactory.Pools.DisjointSetVariableService.Allocate();

            foreach (var variable in oldService.Parameters)
                newService.Parameters.Add(variable);

            foreach (var variable in oldService.Locals)
                newService.Locals.Union(variable, variable);

            foreach (var block in target.Body)
            {
                foreach (var instruction in block)
                {
                    if (instruction is PhiInstruction phi)
                    {
                        foreach (var src in instruction.InputVariables)
                            newService.Locals.Union(phi.Output, src);
                    }
                }
            }

            target.SetService<IVariableService>(newService);
        }
    }
}
