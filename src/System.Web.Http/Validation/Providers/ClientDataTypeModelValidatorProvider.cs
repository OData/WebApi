using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;

namespace System.Web.Http.Validation.Providers
{
    public class ClientDataTypeModelValidatorProvider : ModelValidatorProvider
    {
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>(new Type[]
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        });

        private static string _resourceClassKey;

        public static string ResourceClassKey
        {
            get { return _resourceClassKey ?? String.Empty; }
            set { _resourceClassKey = value; }
        }

        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            return GetValidatorsImpl(metadata, validatorProviders);
        }

        private static IEnumerable<ModelValidator> GetValidatorsImpl(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            Type type = metadata.ModelType;

            if (IsDateTimeType(type))
            {
                yield return new DateModelValidator(metadata, validatorProviders);
            }

            if (IsNumericType(type))
            {
                yield return new NumericModelValidator(metadata, validatorProviders);
            }
        }

        private static bool IsNumericType(Type type)
        {
            return _numericTypes.Contains(GetTypeToValidate(type));
        }

        private static bool IsDateTimeType(Type type)
        {
            return typeof(DateTime) == GetTypeToValidate(type);
        }

        private static Type GetTypeToValidate(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type; // strip off the Nullable<>
        }

        // If the user specified a ResourceClassKey try to load the resource they specified.
        // If the class key is invalid, an exception will be thrown.
        // If the class key is valid but the resource is not found, it returns null, in which
        // case it will fall back to the MVC default error message.
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "resourceName", Justification = "This is temporary")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "actionContext", Justification = "This is temporary")]
        private static string GetUserResourceString(string resourceName)
        {
            string result = null;
            // REVIEW: Removed user-settable resource option
#if false
            if (!String.IsNullOrEmpty(ResourceClassKey) && (HttpExecutionContext != null) && (HttpExecutionContext.HttpContext != null))
            {
                result = HttpExecutionContext.HttpContext.GetGlobalResourceObject(ResourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string;
            }
#endif
            return result;
        }

        private static string GetFieldMustBeNumericResource()
        {
            return GetUserResourceString("FieldMustBeNumeric") ?? SRResources.ClientDataTypeModelValidatorProvider_FieldMustBeNumeric;
        }

        private static string GetFieldMustBeDateResource()
        {
            return GetUserResourceString("FieldMustBeDate") ?? SRResources.ClientDataTypeModelValidatorProvider_FieldMustBeDate;
        }

        internal class ClientModelValidator : ModelValidator
        {
            private string _errorMessage;
            private string _validationType;

            public ClientModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, string validationType, string errorMessage)
                : base(metadata, validatorProviders)
            {
                if (String.IsNullOrEmpty(validationType))
                {
                    throw Error.ArgumentNullOrEmpty("validationType");
                }
                if (String.IsNullOrEmpty(errorMessage))
                {
                    throw Error.ArgumentNullOrEmpty("errorMessage");
                }

                _validationType = validationType;
                _errorMessage = errorMessage;
            }

            public sealed override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
            {
                ModelClientValidationRule rule = new ModelClientValidationRule()
                {
                    ValidationType = _validationType,
                    ErrorMessage = FormatErrorMessage(Metadata.GetDisplayName())
                };

                return new ModelClientValidationRule[] { rule };
            }

            private string FormatErrorMessage(string displayName)
            {
                // use CurrentCulture since this message is intended for the site visitor
                return Error.Format(_errorMessage, displayName);
            }

            public sealed override IEnumerable<ModelValidationResult> Validate(object container)
            {
                // this is not a server-side validator
                return Enumerable.Empty<ModelValidationResult>();
            }
        }

        internal sealed class DateModelValidator : ClientModelValidator
        {
            public DateModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(metadata, validatorProviders, "date", GetFieldMustBeDateResource())
            {
            }
        }

        internal sealed class NumericModelValidator : ClientModelValidator
        {
            public NumericModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(metadata, validatorProviders, "number", GetFieldMustBeNumericResource())
            {
            }
        }
    }
}
