// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Default lower camel caser to resolve property names for <see cref="ODataConventionModelBuilder"/>
    /// </summary>
    public class LowerCamelCaser
    {
        private readonly NameResolverOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LowerCamelCaser"/> class.
        /// </summary>
        public LowerCamelCaser()
        {
            this._options = NameResolverOptions.ApplyToAllProperties;
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
        public void Apply(ODataConventionModelBuilder builder)
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
            stringBuilder.Append(Char.ToLowerInvariant(name[0]));

            for (int index = 1; index < name.Length; index++)
            {
                if (Char.IsUpper(name[index]))
                {
                    stringBuilder.Append(Char.ToLowerInvariant(name[index]));
                }
                else
                {
                    if (Char.IsLower(name[index]) && stringBuilder.Length > 1)
                    {
                        stringBuilder[stringBuilder.Length - 1] =
                            Char.ToUpperInvariant(stringBuilder[stringBuilder.Length - 1]);
                    }

                    stringBuilder.Append(name.Substring(index));
                    break;
                }
            }

            return stringBuilder.ToString();
        }

        private bool ShouldApplyLowerCamelCase(PropertyConfiguration property)
        {
            if (property.AddedExplicitly)
            {
                if (_options.HasFlag(NameResolverOptions.RespectExplicitProperties))
                {
                    return false;
                }
            }
            else if (_options.HasFlag(NameResolverOptions.RespectModelAliasing))
            {
                DataMemberAttribute attribute = property.PropertyInfo.GetCustomAttributes(inherit: true)
                    .OfType<DataMemberAttribute>().SingleOrDefault();

                if (attribute != null && !String.IsNullOrWhiteSpace(attribute.Name))
                {
                    // The property has a resolved name by model aliasing.
                    return false;
                }
            }

            return true;
        }
    }
}
