//-----------------------------------------------------------------------------
// <copyright file="CreatorSettings.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    /// <summary>
    /// Defines global settings for the <see cref="InstanceCreator"/> class.
    /// </summary>
    public class CreatorSettings
    {
        #region Constants and Fields

        internal const bool DefaultCreateDateTimeWithSubMilliseconds = true;

        internal const bool DefaultCreateOnlyAsciiChars = false;

        internal const bool DefaultDontCreateSurrogateChars = true;

        internal const int DefaultMaxArrayLength = 10;

        internal const int DefaultMaxListLength = 10;

        internal const int DefaultMinStringLength = 0;

        internal const int DefaultMaxStringLength = 100;

        internal const double DefaultNullValueProbability = 0.01;

        internal const bool DefaultAllowEmptyCollection = true;

        internal const int DefaultMaxGraphDepth = 10;

        private int currentDepth = 0;

        #endregion

        #region Constructors and Destructors

        public CreatorSettings()
        {
            this.MaxArrayLength = DefaultMaxArrayLength;
            this.MinStringLength = DefaultMinStringLength;
            this.MaxStringLength = DefaultMaxStringLength;
            this.MaxListLength = DefaultMaxListLength;
            this.CreateOnlyAsciiChars = DefaultCreateOnlyAsciiChars;
            this.CreateDateTimeWithSubMilliseconds = DefaultCreateDateTimeWithSubMilliseconds;
            this.DontCreateSurrogateChars = DefaultDontCreateSurrogateChars;
            this.NullValueProbability = DefaultNullValueProbability;
            this.AllowEmptyCollection = DefaultAllowEmptyCollection;
            this.MaxGraphDepth = DefaultMaxGraphDepth;
        }

        public CreatorSettings(CreatorSettings other)
        {
            this.MaxArrayLength = other.MaxArrayLength;
            this.MinStringLength = other.MinStringLength;
            this.MaxStringLength = other.MaxArrayLength;
            this.MaxListLength = other.MaxListLength;
            this.CreateOnlyAsciiChars = other.CreateOnlyAsciiChars;
            this.CreateDateTimeWithSubMilliseconds = other.CreateDateTimeWithSubMilliseconds;
            this.DontCreateSurrogateChars = other.DontCreateSurrogateChars;
            this.NullValueProbability = other.NullValueProbability;
            this.AllowEmptyCollection = other.AllowEmptyCollection;
            this.MaxGraphDepth = other.MaxGraphDepth;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Flag indicating whether the <see cref="InstanceCreator"/> will create DateTime instances
        /// with sub-millisecond precision. This is usually set to false to test serialization formats
        /// whose precision for those types are up to the millisecond, such as the
        /// DataContractJsonSerializer.
        /// </summary>
        public bool CreateDateTimeWithSubMilliseconds { get; set; }

        /// <summary>
        /// Flag indicating whether the <see cref="InstanceCreator"/> should only use ASCII
        /// characters when creating strings or char instances.
        /// </summary>
        public bool CreateOnlyAsciiChars { get; set; }

        /// <summary>
        /// An instance of a surrogate which can be used to create special values for types in an
        /// object graph which is created by the <see cref="InstanceCreator"/>.
        /// </summary>
        public InstanceCreatorSurrogate CreatorSurrogate { get; set; }

        /// <summary>
        /// Flag indicating whether the <see cref="InstanceCreator"/> should only create surrogate
        /// characters when creating strings instances.
        /// </summary>
        public bool DontCreateSurrogateChars { get; set; }

        /// <summary>
        /// The maximum length of arrays created by the <see cref="InstanceCreator"/>.
        /// </summary>
        public int MaxArrayLength { get; set; }

        /// <summary>
        /// The maximum number of elements in List&lt;T&gt; objects created by the <see cref="InstanceCreator"/>.
        /// </summary>
        public int MaxListLength { get; set; }

        /// <summary>
        /// The minimum length of strings created by the <see cref="InstanceCreator"/>.
        /// </summary>
        public int MinStringLength { get; set; }

        /// <summary>
        /// The maximum length of strings created by the <see cref="InstanceCreator"/>.
        /// </summary>
        public int MaxStringLength { get; set; }

        /// <summary>
        /// The likelyhood of the <see cref="InstanceCreator"/> to return <code>null</code> when creating
        /// instances of reference types.
        /// </summary>
        public double NullValueProbability { get; set; }

        /// <summary>
        /// Should <see cref="InstanceCreator"/> create zero size arrays or lists.
        /// </summary>
        public bool AllowEmptyCollection { get; set; }

        /// <summary>
        /// The max depth for generated object graph by the <see cref="InstanceCreator"/>.
        /// </summary>
        public int MaxGraphDepth { get; set; }

        #endregion

        internal bool EnterRecursion()
        {
            if (currentDepth >= this.MaxGraphDepth)
            {
                return false;
            }

            currentDepth++;
            return true;
        }

        internal void LeaveRecursion()
        {
            currentDepth--;
        }
    }
}
