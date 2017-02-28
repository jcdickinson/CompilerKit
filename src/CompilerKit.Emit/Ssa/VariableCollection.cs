using CompilerKit.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a collection of <see cref="Variable"/> instances.
    /// </summary>
    /// <seealso cref="IEnumerable{Variable}" />
    /// <seealso cref="IReadOnlyDictionary{string, Variable}" />
    public struct VariableCollection : IEnumerable<Variable>, IReadOnlyDictionary<string, Variable>
    {
        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="Variable"/> with the specified key.</returns>
        public Variable this[string key] => _dictionary[key];

        /// <summary>
        /// Gets the number of <see cref="Variable"/> contained by the collection.
        /// </summary>
        /// <value>
        /// The number of <see cref="Variable"/> contained by the collection.
        /// </value>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets the collection of keys contained by the collection.
        /// </summary>
        /// <value>
        /// The collection of keys contained by the collection.
        /// </value>
        public IEnumerable<string> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets the collection of values contained by the collection.
        /// </summary>
        /// <value>
        /// The collection of values contained by the collection.
        /// </value>
        public IEnumerable<Variable> Values => _dictionary.Values;

        private readonly Dictionary<string, Variable> _dictionary;
        private readonly List<Variable> _order;
        private readonly string _prefix;
        private readonly bool _isParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> struct.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="isParameters">If set to <c>true</c>, the collection contains parameters.</param>
        internal VariableCollection(string prefix, bool isParameters)
        {
            _dictionary = new Dictionary<string, Variable>(StringComparer.Ordinal);
            _order = new List<Variable>();
            _prefix = prefix;
            _isParameters = isParameters;
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        internal void Clear()
        {
            if (_order.Count != 0)
            {
                for (var i = 0; i < _order.Count && _order[i].Free(); i++) ;
                _order.Clear();
                _dictionary.Clear();
            }
        }

        /// <summary>
        /// Creates and adds a new <see cref="Variable"/> to the collection.
        /// </summary>
        /// <param name="type">The type of the <see cref="Variable"/>.</param>
        /// <returns>The <see cref="Variable"/>.</returns>
        public Variable Add(Type type) => Add(type, $"{_prefix}{_dictionary.Count}");

        /// <summary>
        /// Creates and adds a new <see cref="Variable" /> to the collection.
        /// </summary>
        /// <param name="type">The type of the <see cref="Variable" />.</param>
        /// <param name="name">The name of the <see cref="Variable"/>.</param>
        /// <returns>
        /// The <see cref="Variable" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Either <paramref name="type"/> is null, or <c>this</c> is an empty instance of <see cref="VariableCollection"/>.
        /// </exception>
        public Variable Add(Type type, string name)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (_dictionary == null) throw new ArgumentNullException("this");

            var variable = SsaFactory.Variable(name, type, type.GetTypeInfo(), _isParameters, _dictionary.Count);
            _dictionary.Add(name, variable);
            _order.Add(variable);
            return variable;
        }

        /// <summary>
        /// Creates and adds a new <see cref="Variable" /> to the collection.
        /// </summary>
        /// <param name="factory">The <see cref="VariableFactory"/> to retrieve a new variable from.</param>
        /// <returns>
        /// The <see cref="Variable" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="type" /> is null, or <c>this</c> is an empty instance of <see cref="VariableCollection" />.</exception>
        public Variable Add(VariableFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var variable = SsaFactory.Variable(factory.NextName, factory.Type, factory.TypeInfo, _isParameters, _dictionary.Count);
            _dictionary.Add(variable.Name, variable);
            _order.Add(variable);
            return variable;
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(string key, out Variable value) =>_dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public ReadOnlyListEnumerator<Variable, List<Variable>> GetEnumerator() => 
            new ReadOnlyListEnumerator<Variable, List<Variable>>(_order);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<KeyValuePair<string, Variable>> IEnumerable<KeyValuePair<string, Variable>>.GetEnumerator() => _dictionary.GetEnumerator();
    }
}
