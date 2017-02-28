using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a block of instructions.
    /// </summary>
    /// <seealso cref="Collection{Instruction}" />
    public class Block : Collection<Instruction>
    {
        /// <summary>
        /// Gets an array of blocks that is empty.
        /// </summary>
        /// <value>
        /// The array of blocks that is empty.
        /// </value>
        public static Block[] EmptyBlocks { get; } = new Block[0];

        /// <summary>
        /// Gets an array of blocks that is empty, as a <see cref="ImmutableArray{Block}"/>.
        /// </summary>
        /// <value>
        /// The array of blocks that is empty, as a <see cref="ImmutableArray{Block}"/>.
        /// </value>
        public static ImmutableArray<Block> ImmutableEmptyBlocks { get; } = EmptyBlocks.ToImmutableArray();

        /// <summary>
        /// Gets the body that the block is contained by.
        /// </summary>
        /// <value>
        /// The body that the block is contained by.
        /// </value>
        public Body Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Block" /> class.
        /// </summary>
        /// <param name="body">The body that contains the block.</param>
        internal Block() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="body">The containing body.</param>
        /// <returns>The <see cref="Block"/> instance.</returns>
        protected internal Block Allocate(Body body)
        {
            Body = body;
            return this;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>
        /// A value indicating whether this instance was returned to the pool.
        /// </returns>
        internal bool Free()
        {
            Body = null;
            if (Count != 0)
            {
                for (var i = 0; i < Count && this[i].Free(); i++) ;
                Clear();
            }
            return SsaFactory.Pools.Block.Free(this);
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        protected override void InsertItem(int index, Instruction item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Block != null && !ReferenceEquals(item.Block, this))
                throw new ArgumentOutOfRangeException(nameof(item), Exceptions.InvalidOperation_InstructionParented);
            item.Block = this;
            item.Index = index;
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        protected override void SetItem(int index, Instruction item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Block != null && !ReferenceEquals(item.Block, this))
                throw new ArgumentOutOfRangeException(nameof(item), Exceptions.InvalidOperation_InstructionParented);
            this[index].Block = null;
            this[index].Index = -1;
            item.Block = this;
            item.Index = index;
            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Block = null;
                item.Index = -1;
            }
            base.ClearItems();
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            this[index].Block = null;
            this[index].Index = -1;
            base.RemoveItem(index);
        }
    }
}
