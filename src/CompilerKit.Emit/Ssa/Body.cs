using CompilerKit.Collections.Generic;
using System.Collections;
using System.Collections.Generic;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a method body that comprises of variables and instructions.
    /// </summary>
    public partial class Body : IReadOnlyList<Block>, IPooledObject
    {
        /// <summary>
        /// Gets the list of parameter variables.
        /// </summary>
        /// <value>
        /// The list of parameter variables.
        /// </value>
        public VariableCollection Parameters { get; }

        /// <summary>
        /// Gets the list of local variables.
        /// </summary>
        /// <value>
        /// The list of local variables.
        /// </value>
        public VariableCollection Locals { get; }

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

        private readonly List<Block> _blocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        internal Body()
        {
            Parameters = new VariableCollection("p", true);
            Locals = new VariableCollection("v", false);
            _blocks = new List<Block>();
        }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <returns>The allocated <see cref="Body"/>.</returns>
        protected internal Body Allocate()
        {
            _blocks.Add(SsaFactory.Block(this));
            return this;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        public bool Free()
        {
            Parameters.Clear();
            Locals.Clear();

            if (_blocks.Count != 0)
            {
                for (var i = 0; i < _blocks.Count && _blocks[i].Free(); i++) ;
                _blocks.Clear();
            }

            return SsaFactory.Pools.Body.Free(this);
        }

        /// <summary>
        /// Creates and adds a new <see cref="Block"/> to the collection.
        /// </summary>
        /// <returns>The <see cref="Block"/>.</returns>
        public Block AddBlock()
        {
            var result = SsaFactory.Block(this);
            _blocks.Add(result);
            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public ListEnumerator<Block, List<Block>> GetEnumerator() => new ListEnumerator<Block, List<Block>>(_blocks);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<Block> IEnumerable<Block>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
