using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace CompilerKit.Emit.Ssa
{
    partial class Body
    {
        private class RootVariableCollection : IRootVariableCollection
        {
            public RootVariable this[string key]
            {
                get { return _dictionary[key]; }
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public IEnumerable<string> Keys
            {
                get { return _dictionary.Keys; }
            }

            public IEnumerable<RootVariable> Values
            {
                get { return _dictionary.Values; }
            }

            public object XmlConvert { get; private set; }

            private readonly Dictionary<string, RootVariable> _dictionary;
            private readonly List<RootVariable> _order;
            private readonly string _prefix;
            private readonly bool _isParameters;

            public RootVariableCollection(string prefix, bool isParameters)
            {
                _dictionary = new Dictionary<string, RootVariable>(StringComparer.Ordinal);
                _order = new List<RootVariable>();
                _prefix = prefix;
                _isParameters = isParameters;
            }

            public RootVariable Add(Type type)
            {
                var name = string.Concat(_prefix, _dictionary.Count.ToString(CultureInfo.InvariantCulture));
                return Add(type, name);
            }

            public RootVariable Add(Type type, string name)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                var variable = new RootVariable(name, type, _isParameters, _dictionary.Count);
                _dictionary.Add(name, variable);
                _order.Add(variable);
                return variable;
            }

            public int IndexOf(Variable variable)
            {
                // PROFILE: Yes, optimize this by moving to Add/Remove.
                var index = 0;
                for (var i = 0; i < _order.Count; i++)
                {
                    var rootCandidate = _order[i];
                    if (_isParameters)
                    {
                        if (rootCandidate == variable.RootVariable)
                            return i;
                    }
                    if (variable.RootVariable == rootCandidate)
                    {
                        for (var j = 0; j < rootCandidate.Variables.Count; j++)
                        {
                            if (rootCandidate.Variables[j] == variable)
                                return index;
                            index++;
                        }
                    }
                    else
                    {
                        index += rootCandidate.Variables.Count;
                    }
                }

                return -1;
            }

            public bool ContainsKey(string key)
            {
                return _dictionary.ContainsKey(key);
            }

            public bool TryGetValue(string key, out RootVariable value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public IEnumerator<RootVariable> GetEnumerator()
            {
                return _order.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<KeyValuePair<string, RootVariable>> IEnumerable<KeyValuePair<string, RootVariable>>.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }
        }
    }
}
