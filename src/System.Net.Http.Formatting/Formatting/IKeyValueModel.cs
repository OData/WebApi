using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Interface to provide a key/value model of an object graph.
    /// </summary>
    /// <remarks>
    /// This interface is used primarily to provide a simple key/value facade over
    /// richer DOM's such as <see cref="T:System.Xml.Linq.XElement"/> or 
    /// <see cref="T:System.Json.JsonValue"/>
    /// </remarks>
    public interface IKeyValueModel
    {
        /// <summary>
        /// Gets all the keys for all the values.
        /// </summary>
        /// <returns>The set of all keys.</returns>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Attempts to retrieve the value associated with the given key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="value">The value associated with that key.</param>
        /// <returns><c>If there was a value associated with that key</c></returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "This is a non-generic API.")]
        bool TryGetValue(string key, out object value);
    }
}
