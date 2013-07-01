// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    /// <summary>
    /// Basic implementation for attributes used for defining routes on an action, optionally specifying that the action supports particular HTTP methods.
    /// </summary>
    internal class HttpVerbsValidator
    {
        private readonly ICollection<string> _verbs;
        private static readonly ConcurrentDictionary<HttpVerbs, ReadOnlyCollection<string>> _verbsToVerbCollections = new ConcurrentDictionary<HttpVerbs, ReadOnlyCollection<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsValidator" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public HttpVerbsValidator(HttpVerbs verbs)
            : this(ConvertVerbs(verbs))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsValidator" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public HttpVerbsValidator(IList<string> verbs)
        {
            ValidateVerbs(verbs);
            _verbs = verbs as ReadOnlyCollection<string>;
            if (_verbs == null)
            {
                _verbs = new ReadOnlyCollection<string>(verbs);
            }
        }

        /// <summary>
        /// Gets the set of allowed HTTP methods for that route. If the route allow any method to be used, the value is null.
        /// </summary>
        public ICollection<string> Verbs
        {
            get { return _verbs; }
        }

        /// <inheritdoc />
        public bool IsValidForRequest(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (_verbs == null)
            {
                return true;
            }

            string incomingVerb = controllerContext.HttpContext.Request.GetHttpMethodOverride();

            return _verbs.Contains(incomingVerb, StringComparer.OrdinalIgnoreCase);
        }

        private static ReadOnlyCollection<string> ConvertVerbs(HttpVerbs verbs)
        {
            ReadOnlyCollection<string> verbsAsCollection;
            if (_verbsToVerbCollections.TryGetValue(verbs, out verbsAsCollection))
            {
                return verbsAsCollection;
            }

            verbsAsCollection = new ReadOnlyCollection<string>(EnumToArray(verbs));
            _verbsToVerbCollections.TryAdd(verbs, verbsAsCollection);
            return verbsAsCollection;
        }

        private static void ValidateVerbs(ICollection<string> verbs)
        {
            if (verbs == null || verbs.Count == 0)
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "verbs");
            }
        }

        internal static string[] EnumToArray(HttpVerbs verbs)
        {
            List<string> verbList = new List<string>();

            AddEntryToList(verbs, HttpVerbs.Get, verbList, "GET");
            AddEntryToList(verbs, HttpVerbs.Post, verbList, "POST");
            AddEntryToList(verbs, HttpVerbs.Put, verbList, "PUT");
            AddEntryToList(verbs, HttpVerbs.Delete, verbList, "DELETE");
            AddEntryToList(verbs, HttpVerbs.Head, verbList, "HEAD");
            AddEntryToList(verbs, HttpVerbs.Patch, verbList, "PATCH");
            AddEntryToList(verbs, HttpVerbs.Options, verbList, "OPTIONS");

            return verbList.ToArray();
        }

        private static void AddEntryToList(HttpVerbs verbs, HttpVerbs match, List<string> verbList, string entryText)
        {
            if ((verbs & match) != 0)
            {
                Contract.Assert(verbList != null);
                verbList.Add(entryText);
            }
        }
    }
}