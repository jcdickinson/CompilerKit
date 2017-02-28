using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Wraps a <see cref="IList{T}"/> as a <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <typeparam name="TList">The type of the list.</typeparam>
    /// <seealso cref="System.Collections.Generic.IReadOnlyList{TElement}" />
    public struct ReadOnlyList<TElement, TList> : IReadOnlyList<TElement>
        where TList : IList<TElement>
    {
        /// <summary>
        /// Gets the <see cref="TElement"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="TElement"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="TElement"/> at the specified index.</returns>
        public TElement this[int index] => _list[index];

        /// <summary>
        /// Gets the number of items contained by this collection.
        /// </summary>
        /// <value>
        /// The number of items contained by this collection.
        /// </value>
        public int Count => _list.Count;

        private readonly TList _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyList{TElement, TList}"/> struct.
        /// </summary>
        /// <param name="list">The contained list.</param>
        public ReadOnlyList(TList list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            _list = list;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public ListEnumerator<TElement, TList> GetEnumerator() => new ListEnumerator<TElement, TList>(_list);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
