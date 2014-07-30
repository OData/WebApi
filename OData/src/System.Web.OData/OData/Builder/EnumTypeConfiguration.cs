// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmEnumType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class EnumTypeConfiguration : IEdmTypeConfiguration
    {
        private const string DefaultNamespace = "Default";
        private string _namespace;
        private string _name;
        private NullableEnumTypeConfiguration nullableEnumTypeConfiguration = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeConfiguration"/> class.
        /// </summary>
        public EnumTypeConfiguration(ODataModelBuilder builder, Type clrType)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            if (!clrType.IsEnum)
            {
                throw Error.Argument("clrType", SRResources.TypeCannotBeEnum, clrType.FullName);
            }

            ClrType = clrType;
            IsFlags = clrType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
            UnderlyingType = Enum.GetUnderlyingType(clrType);
            ModelBuilder = builder;
            _name = clrType.EdmName();
            _namespace = clrType.Namespace ?? DefaultNamespace;
            ExplicitMembers = new Dictionary<Enum, EnumMemberConfiguration>();
            RemovedMembers = new List<Enum>();
        }

        /// <summary>
        /// Gets the <see cref="EdmTypeKind"/> of this EDM type.
        /// </summary>
        public EdmTypeKind Kind
        {
            get
            {
                return EdmTypeKind.Enum;
            }
        }

        /// <summary>
        /// Gets the <see cref="IsFlags"/> of this enum type. 
        /// If it is true, a combined value is equivalent to the bitwise OR of the discrete values.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "It is clearer to use IsFlags here and it is corresponding to the Flags attribute")]
        public bool IsFlags { get; private set; }

        /// <summary>
        /// Gets the backing CLR <see cref="Type"/>.
        /// </summary>
        public Type ClrType { get; private set; }

        /// <summary>
        /// Gets this enum underlying <see cref="Type"/>.
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Gets the full name of this EDM type.
        /// </summary>
        public string FullName
        {
            get
            {
                return Namespace + "." + Name;
            }
        }

        /// <summary>
        /// Gets or sets the namespace of this EDM type.
        /// </summary>
        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _namespace = value;
                AddedExplicitly = true;
            }
        }

        /// <summary>
        /// Gets or sets the name of this EDM type.
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
                AddedExplicitly = true;
            }
        }

        /// <summary>
        /// Gets all possible members(defined values) of this enum type, which will be added to the EDM model as edm:Member elements.
        /// </summary>
        public IEnumerable<EnumMemberConfiguration> Members
        {
            get
            {
                return ExplicitMembers.Values;
            }
        }

        /// <summary>
        /// Gets the members from the backing CLR type that are to be ignored on this enum type.
        /// </summary>
        public ReadOnlyCollection<Enum> IgnoredMembers
        {
            get
            {
                return new ReadOnlyCollection<Enum>(RemovedMembers);
            }
        }

        /// <summary>
        /// Gets or sets a value that is <see langword="true"/> if the type's name or namespace was set by the user; 
        /// <see langword="false"/> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <see langword="false"/>.</remarks>
        public bool AddedExplicitly { get; set; }

        /// <summary>
        /// Get the <see cref="ODataModelBuilder"/>.
        /// </summary>
        public ODataModelBuilder ModelBuilder { get; private set; }

        /// <summary>
        /// Gets the collection of explicitly removed members.
        /// </summary>
        protected internal IList<Enum> RemovedMembers { get; private set; }

        /// <summary>
        /// Gets the collection of explicitly added members.
        /// </summary>
        protected internal IDictionary<Enum, EnumMemberConfiguration> ExplicitMembers { get; private set; }

        /// <summary>
        /// Adds an enum member to this enum type.
        /// </summary>
        /// <param name="member">The member being added.</param>
        /// <returns>The <see cref="EnumMemberConfiguration"/> so that the member can be configured further.</returns>
        public EnumMemberConfiguration AddMember(Enum member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            if (member.GetType() != ClrType)
            {
                throw Error.Argument("member", SRResources.PropertyDoesNotBelongToType, member.ToString(), ClrType.FullName);
            }

            // Remove from the ignored members
            if (RemovedMembers.Contains(member))
            {
                RemovedMembers.Remove(member);
            }

            EnumMemberConfiguration memberConfiguration;
            if (ExplicitMembers.ContainsKey(member))
            {
                memberConfiguration = ExplicitMembers[member];
            }
            else
            {
                memberConfiguration = new EnumMemberConfiguration(member, this);
                ExplicitMembers[member] = memberConfiguration;
            }

            return memberConfiguration;
        }

        /// <summary>
        /// Removes the given member.
        /// </summary>
        /// <param name="member">The member being removed.</param>
        public void RemoveMember(Enum member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            if (member.GetType() != ClrType)
            {
                throw Error.Argument("member", SRResources.PropertyDoesNotBelongToType, member.ToString(), ClrType.FullName);
            }

            if (ExplicitMembers.ContainsKey(member))
            {
                ExplicitMembers.Remove(member);
            }

            if (!RemovedMembers.Contains(member))
            {
                RemovedMembers.Add(member);
            }
        }

        internal NullableEnumTypeConfiguration GetNullableEnumTypeConfiguration()
        {
            if (nullableEnumTypeConfiguration == null)
            {
                nullableEnumTypeConfiguration = new NullableEnumTypeConfiguration(this);
            }

            return nullableEnumTypeConfiguration;
        }
    }
}
