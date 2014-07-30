// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Default lower camel caser to resolve property names for <see cref="ODataConventionModelBuilder"/>.
    /// The rule is to convert the leading upper case characters to lower case, 
    /// until a character, which is not the first character and is followed by a non-upper case character, is met.
    /// id => id, ID => id, MyName => myName, IOStream => ioStream, MyID => myid, yourID => yourID
    /// </summary>
    public class LowerCamelCaser
    {
        private readonly NameResolverOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LowerCamelCaser"/> class.
        /// </summary>
        public LowerCamelCaser()
            : this(NameResolverOptions.ProcessReflectedPropertyNames |
                NameResolverOptions.ProcessDataMemberAttributePropertyNames |
                NameResolverOptions.ProcessExplicitPropertyNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowerCamelCaser"/> class.
        /// </summary>
        /// <param name="options">Name resolver options for camelizing.</param>
        public LowerCamelCaser(NameResolverOptions options)
        {
            this._options = options;
        }

        /// <summary>
        /// Applies lower camel case.
        /// </summary>
        /// <param name="builder">The <see cref="ODataConventionModelBuilder"/> to be applied on.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public void ApplyLowerCamelCase(ODataConventionModelBuilder builder)
        {
            foreach (StructuralTypeConfiguration typeConfiguration in builder.StructuralTypes)
            {
                foreach (PropertyConfiguration property in typeConfiguration.Properties)
                {
                    if (ShouldApplyLowerCamelCase(property))
                    {
                        property.Name = ToLowerCamelCase(property.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a <see cref="string"/> to lower camel case.
        /// </summary>
        /// <param name="name">The name to be converted with lower camel case.</param>
        /// <returns>The converted name.</returns>
        public virtual string ToLowerCamelCase(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return name;
            }

            if (!Char.IsUpper(name[0]))
            {
                return name;
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int index = 0; index < name.Length; index++)
            {
                if (index != 0 && index + 1 < name.Length && !Char.IsUpper(name[index + 1]))
                {
                    stringBuilder.Append(name.Substring(index));
                    break;
                }
                else
                {
                    stringBuilder.Append(Char.ToLower(name[index], CultureInfo.InvariantCulture));
                }
            }

            return stringBuilder.ToString();
        }

        private bool ShouldApplyLowerCamelCase(PropertyConfiguration property)
        {
            if (property.AddedExplicitly)
            {
                return _options.HasFlag(NameResolverOptions.ProcessExplicitPropertyNames);
            }
            else
            {
                DataMemberAttribute attribute = property.PropertyInfo.GetCustomAttribute<DataMemberAttribute>(inherit: false);

                if (attribute != null && !String.IsNullOrWhiteSpace(attribute.Name))
                {
                    return _options.HasFlag(NameResolverOptions.ProcessDataMemberAttributePropertyNames);
                }

                return _options.HasFlag(NameResolverOptions.ProcessReflectedPropertyNames);
            }
        }
    }
}
