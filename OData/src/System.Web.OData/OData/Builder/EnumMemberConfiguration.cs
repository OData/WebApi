// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents the configuration for an enum member of an enum type.
    /// </summary>
    public class EnumMemberConfiguration
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMemberConfiguration"/> class.
        /// </summary>
        /// <param name="member">The member of the enum type.</param>
        /// <param name="declaringType">The declaring type of the member.</param>
        public EnumMemberConfiguration(Enum member, EnumTypeConfiguration declaringType)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            if (declaringType == null)
            {
                throw Error.ArgumentNull("declaringType");
            }

            Contract.Assert(member.GetType() == declaringType.ClrType);

            MemberInfo = member;
            DeclaringType = declaringType;
            AddedExplicitly = true;
            _name = Enum.GetName(member.GetType(), member);
        }

        /// <summary>
        /// Gets or sets the name of the member.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        public EnumTypeConfiguration DeclaringType { get; private set; }

        /// <summary>
        /// Gets the mapping CLR <see cref="Enum"/>.
        /// </summary>
        public Enum MemberInfo { get; private set; }

        /// <summary>
        /// Gets or sets a value that is <see langword="true"/> if the member was added by the user; <see langword="false"/> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <see langword="true"/></remarks>
        public bool AddedExplicitly { get; set; }
    }
}