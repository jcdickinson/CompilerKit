using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a method body that comprises of variables and instructions.
    /// </summary>
    public partial class Body : Collection<Instruction>
    {
        /// <summary>
        /// Gets the list of parameter variables.
        /// </summary>
        /// <value>
        /// The list of parameter variables.
        /// </value>
        public IRootVariableCollection Parameters { get; }

        private readonly RootVariableCollection _variables;
        /// <summary>
        /// Gets the list of variables.
        /// </summary>
        /// <value>
        /// The list of variables.
        /// </value>
        public IRootVariableCollection Variables { get { return _variables; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        public Body()
             : this(new List<Instruction>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="list">The list of existing instructions.</param>
        public Body(IList<Instruction> list)
            : base(list)
        {
            Parameters = new RootVariableCollection("p", true);
            _variables = new RootVariableCollection("v", false);
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
            if (item.Body != null && !ReferenceEquals(item.Body, this))
                throw new ArgumentOutOfRangeException(nameof(item), Properties.Resources.InvalidOperation_InstructionParented);
            item.Body = this;
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
            if (item.Body != null && !ReferenceEquals(item.Body, this))
                throw new ArgumentOutOfRangeException(nameof(item), Properties.Resources.InvalidOperation_InstructionParented);
            this[index].Body = null;
            item.Body = this;
            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in this)
                item.Body = null;
            base.ClearItems();
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            this[index].Body = null;
            base.RemoveItem(index);
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator"/>.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/> to compile to.</param>
        public void CompileTo(ILGenerator il)
        {
            foreach (var variable in ((IEnumerable<RootVariable>)_variables).SelectMany(x => x.Variables))
            {
                il.DeclareLocal(variable.Type);
            }

            foreach (var instruction in this)
            {
                instruction.CompileTo(il);
            }
        }
    }
}
