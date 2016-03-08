namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents information about a label.
    /// </summary>
    public interface ILabel
    {
        /// <summary>
        /// Gets the name of the label.
        /// </summary>
        /// <value>
        /// The name of the label.
        /// </value>
        string Name { get; }
    }
}
