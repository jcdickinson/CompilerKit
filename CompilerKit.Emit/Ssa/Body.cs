﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a method body that comprises of variables and instructions.
    /// </summary>
    public partial class Body : IReadOnlyList<Block>
    {
        /// <summary>
        /// Gets the list of parameter variables.
        /// </summary>
        /// <value>
        /// The list of parameter variables.
        /// </value>
        public IRootVariableCollection Parameters { get; }

        /// <summary>
        /// Gets the list of variables.
        /// </summary>
        /// <value>
        /// The list of variables.
        /// </value>
        public IRootVariableCollection Variables { get; }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => _blocks.Count;

        /// <summary>
        /// Gets the <see cref="Block"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Block"/>.
        /// </value>
        /// <param name="index">The index of the see cref="Block"/> to get.</param>
        /// <returns>The <see cref="Block"/> at the specified index.</returns>
        /// <summary>
        /// Gets the <see cref="Block"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Block"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Block this[int index] => _blocks[index];

        /// <summary>
        /// Gets the main <see cref="Block"/> that contains the initial instructions contained
        /// by the method.
        /// </summary>
        public Block MainBlock => _blocks[0];

        private readonly IList<Block> _blocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        public Body()
        {
            Parameters = new RootVariableCollection("p", true);
            Variables = new RootVariableCollection("v", false);
            _blocks = new List<Block>();
            _blocks.Add(new Block(this));
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator"/>.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/> to compile to.</param>
        public void CompileTo(ILGenerator il)
        {
            foreach (var variable in ((IEnumerable<RootVariable>)Variables).SelectMany(x => x.Variables))
            {
                il.DeclareLocal(variable.Type);
            }

            foreach (var instruction in MainBlock)
            {
                instruction.CompileTo(il);
            }
        }

        /// <summary>
        /// Optimizes the method body with the specified optimizers.
        /// </summary>
        /// <param name="optimizers">The optimizers to optimize the method body with.</param>
        public void Optimize(params Action<Body>[] optimizers)
        {
            Optimize((IEnumerable<Action<Body>>)optimizers);
        }

        /// <summary>
        /// Optimizes the method body with the specified optimizers.
        /// </summary>
        /// <param name="optimizers">The optimizers to optimize the method body with.</param>
        public void Optimize(IEnumerable<Action<Body>> optimizers)
        {
            if (optimizers == null) throw new ArgumentNullException(nameof(optimizers));
            foreach (var optimizer in optimizers)
            {
                if (optimizer == null) throw new ArgumentNullException(nameof(optimizers));
                optimizer(this);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Block> GetEnumerator()
        {
            return _blocks.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
