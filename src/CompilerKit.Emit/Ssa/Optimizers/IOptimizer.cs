namespace CompilerKit.Emit.Ssa.Optimizers
{
    /// <summary>
    /// Represents the core implementation of an optimizer.
    /// </summary>
    public interface IOptimizer
    {
        /// <summary>
        /// Optimizes the specified target.
        /// </summary>
        /// <param name="target">The target to optimize.</param>
        void Optimize(IBodyTarget target);
    }
}
