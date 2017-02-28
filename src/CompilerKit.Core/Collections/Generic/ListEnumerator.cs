using System;
using System.Collections;
using System.Collections.Generic;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Represents a struct enumerator for <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="TList">The type of the list.</typeparam>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public struct ListEnumerator<TElement, TList> : IEnumerator<TElement>
        where TList : IList<TElement>
    {
        private int _index;
        private readonly TList _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyListEnumerator{TList, TElement}"/> struct.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <exception cref="System.ArgumentNullException">list</exception>
        public ListEnumerator(TList list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            _list = list;
            _index = -1;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value>
        /// The element in the collection at the current position of the enumerator.
        /// </value>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value>
        /// The element in the collection at the current position of the enumerator.
        /// </value>
        public TElement Current => _list[_index];

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext() => ++_index < _list.Count;

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset() => _index = -1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }
}
