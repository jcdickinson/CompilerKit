namespace CompilerKit.Emit
{
    /// <summary>
    /// Represents a pooled instance.
    /// </summary>
    public interface IPooledObject
    {
        /// <summary>
        /// Returns this instance to the pool.
        /// </summary>
        /// <returns>A value indicating whether the instance was returned to the pool.</returns>
        bool Free();
    }
}
