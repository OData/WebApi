// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the catch block location for an <see cref="ExceptionContext"/>.</summary>
    [DebuggerDisplay("Name: {Name}, IsTopLevel: {IsTopLevel}")]
    public class ExceptionContextCatchBlock
    {
        private readonly string _name;
        private readonly bool _isTopLevel;

        /// <summary>
        /// Initializes a new instance of <see cref="ExceptionContextCatchBlock"/> with the values provided.
        /// </summary>
        /// <param name="name">The label for the catch block where the exception was caught.</param>
        /// <param name="isTopLevel">
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </param>
        /// <remarks>
        /// To compare an exception catch block with a well-known value, see classes like
        /// <see cref="ExceptionCatchBlocks"/> for the specific objects to use.
        /// This constructor is only intended for use within static classes that define such well-known catch blocks.
        /// </remarks>
        public ExceptionContextCatchBlock(string name, bool isTopLevel)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            _name = name;
            _isTopLevel = isTopLevel;
        }

        /// <summary>Gets a label for the catch block in which the exception was caught.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets a value indicating whether the catch block where the exception was caught is the last one before the
        /// host.
        /// </summary>
        public bool IsTopLevel
        {
            get { return _isTopLevel; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _name;
        }
    }
}
