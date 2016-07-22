using System;
using System.Collections.ObjectModel;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a block of instructions.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.Collection{CompilerKit.Emit.Ssa.Instruction}" />
    public class Block : Collection<Instruction>
    {
        /// <summary>
        /// Gets the body that the block is contained by.
        /// </summary>
        /// <value>
        /// The body that the block is contained by.
        /// </value>
        public Body Body { get; }

        /// <summary>
        /// Gets the list of parameter variables from the containing body.
        /// </summary>
        /// <value>
        /// The list of parameter variables from the containing body.
        /// </value>
        public IRootVariableCollection Parameters => Body.Parameters;

        /// <summary>
        /// Gets the list of variables from the containing body.
        /// </summary>
        /// <value>
        /// The list of variables from the containing body.
        /// </value>
        public IRootVariableCollection Variables => Body.Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Block" /> class.
        /// </summary>
        /// <param name="body">The body that contains the block.</param>
        internal Block(Body body)
        {
            Body = body;
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
                throw new ArgumentOutOfRangeException(nameof(item), Properties.Resources.InvalidOperation_InstructionParented);
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
                throw new ArgumentOutOfRangeException(nameof(item), Properties.Resources.InvalidOperation_InstructionParented);
            this[index].Block = null;
            this[index].Index = -1;
            item.Block = this;
            item.Index = index;
            base.SetItem(index, item);
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="IMethodEmitRequest" /> and
        /// <see cref="IILGenerator" />.
        /// </summary>
        /// <param name="emitRequest">The emit request to compile against.</param>
        /// <param name="il">The <see cref="IILGenerator"/> that will be populated with the final code.</param>
        public virtual void CompileTo(IILGenerator il)
        {
            foreach (var instruction in this)
            {
                instruction.CompileTo(il);
            }
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
