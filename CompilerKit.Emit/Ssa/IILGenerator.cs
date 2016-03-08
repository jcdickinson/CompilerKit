﻿namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an IL generator.
    /// </summary>
    public interface IILGenerator
    {
        /// <summary>
        /// Emits the start of the specified body.
        /// </summary>
        /// <param name="block">The block to declare.</param>
        void Declare(Block block);

        /// <summary>
        /// Emits a declaration of the specified variable.
        /// </summary>
        /// <param name="variable">The variable to declare.</param>
        void Declare(Variable variable);

        /// <summary>
        /// Emits an instruction that loads a stack value from the specified <see cref="Variable"/>.
        /// </summary>
        /// <param name="variable">The variable to load from.</param>
        /// <param name="options">The options that describe how the <see cref="Variable"/> should be loaded.</param>
        void Load(Variable variable, EmitOptions options);

        /// <summary>
        /// Emits an instruction that stores a stack value into the specified <see cref="Variable"/>.
        /// </summary>
        /// <param name="variable">The variable to store into.</param>
        /// <param name="options">The options that describe how the <see cref="Variable"/> should be stored.</param>
        void Store(Variable variable, EmitOptions options);

        /// <summary>
        /// Emits an instruction that returns from the method.
        /// </summary>
        void Return();

        /// <summary>
        /// Emits a binary operator.
        /// </summary>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="options">The options that describe how the operator should behave.</param>
        void Binary(BinaryOperator binaryOperator, EmitOptions options);

        /// <summary>
        /// Emits the preamble for the specified block.
        /// </summary>
        /// <param name="block">The block to emit.</param>
        void Emit(Block block);

        /// <summary>
        /// Emits a branch which will jump to the specified <see cref="Block" />
        /// based on the result of the specified <see cref="Comparison" />.
        /// </summary>
        /// <param name="comparison">The comparison that is used to determine if the branch should occur.</param>
        /// <param name="block">The block to jump to.</param>
        /// <param name="options">The options that describe how the comparison should behave.</param>
        void Branch(Comparison comparison, Block block, EmitOptions options);
    }
}
