using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation.Validators;

namespace System.Web.Http.Validation.Providers
{
    // A factory for validators based on ValidationAttribute
    public delegate ModelValidator DataAnnotationsModelValidationFactory(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute);

    // A factory for validators based on IValidatableObject
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders);

    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    // [SecuritySafeCritical] because class constructor accesses DataAnnotations types
    [SecuritySafeCritical]
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider
    {
        private static ReaderWriterLockSlim _adaptersLock = new ReaderWriterLockSlim();

        // Factories for validation attributes

        internal static DataAnnotationsModelValidationFactory DefaultAttributeFactory =
            (metadata, validationProviders, attribute) => new DataAnnotationsModelValidator(metadata, validationProviders, attribute);

        internal static readonly Dictionary<Type, DataAnnotationsModelValidationFactory> AttributeFactories =
            new Dictionary<Type, DataAnnotationsModelValidationFactory>();

        // Factories for IValidatableObject models
        internal static DataAnnotationsValidatableObjectAdapterFactory DefaultValidatableFactory =
            (metadata, validationProviders) => new ValidatableObjectAdapter(metadata, validationProviders);

        internal static readonly Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> ValidatableFactories =
            new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        // [SecuritySafeCritical] because it uses DataAnnotations type ValidationAttribute and IValidatableObject
        [SecuritySafeCritical]
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
            _adaptersLock.EnterReadLock();

            try
            {
                List<ModelValidator> results = new List<ModelValidator>();

                // Produce a validator for each validation attribute we find
                foreach (ValidationAttribute attribute in attributes.OfType<ValidationAttribute>())
                {
                    DataAnnotationsModelValidationFactory factory;
                    if (!AttributeFactories.TryGetValue(attribute.GetType(), out factory))
                    {
                        factory = DefaultAttributeFactory;
                    }
                    results.Add(factory(metadata, validatorProviders, attribute));
                }

                // Produce a validator if the type supports IValidatableObject
                if (typeof(IValidatableObject).IsAssignableFrom(metadata.ModelType))
                {
                    DataAnnotationsValidatableObjectAdapterFactory factory;
                    if (!ValidatableFactories.TryGetValue(metadata.ModelType, out factory))
                    {
                        factory = DefaultValidatableFactory;
                    }
                    results.Add(factory(metadata, validatorProviders));
                }

                return results;
            }
            finally
            {
                _adaptersLock.ExitReadLock();
            }
        }

        #region Validation attribute adapter registration

        public static void RegisterAdapter(Type attributeType, Type adapterType)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(attributeType, adapterType);

            _adaptersLock.EnterWriteLock();

            try
            {
                AttributeFactories[attributeType] = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
            }
            finally
            {
                _adaptersLock.ExitWriteLock();
            }
        }

        public static void RegisterAdapterFactory(Type attributeType, DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeFactory(factory);

            _adaptersLock.EnterWriteLock();

            try
            {
                AttributeFactories[attributeType] = factory;
            }
            finally
            {
                _adaptersLock.ExitWriteLock();
            }
        }

        public static void RegisterDefaultAdapter(Type adapterType)
        {
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(typeof(ValidationAttribute), adapterType);

            DefaultAttributeFactory = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
        }

        public static void RegisterDefaultAdapterFactory(DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeFactory(factory);

            DefaultAttributeFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetAttributeAdapterConstructor(Type attributeType, Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(ModelMetadata), typeof(HttpActionContext), attributeType });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", SRResources.DataAnnotationsModelValidatorProvider_ConstructorRequirements, adapterType, attributeType);
            }

            return constructor;
        }

        private static void ValidateAttributeAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }

            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", SRResources.Common_TypeMustDriveFromType, adapterType, typeof(ModelValidator));
            }
        }

        private static void ValidateAttributeType(Type attributeType)
        {
            if (attributeType == null)
            {
                throw Error.ArgumentNull("attributeType");
            }

            if (!typeof(ValidationAttribute).IsAssignableFrom(attributeType))
            {
                throw Error.Argument("attributeType", SRResources.Common_TypeMustDriveFromType, attributeType, typeof(ModelValidator));
            }
        }

        private static void ValidateAttributeFactory(DataAnnotationsModelValidationFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }

        #endregion

        #region IValidatableObject adapter registration

        /// <summary>
        /// Registers an adapter type for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HttpActionContext"/>.
        /// </summary>
        public static void RegisterValidatableObjectAdapter(Type modelType, Type adapterType)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            _adaptersLock.EnterWriteLock();

            try
            {
                ValidatableFactories[modelType] = (metadata, context) => (ModelValidator)constructor.Invoke(new object[] { metadata, context });
            }
            finally
            {
                _adaptersLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers an adapter factory for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>.
        /// </summary>
        public static void RegisterValidatableObjectAdapterFactory(Type modelType, DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableFactory(factory);

            _adaptersLock.EnterWriteLock();

            try
            {
                ValidatableFactories[modelType] = factory;
            }
            finally
            {
                _adaptersLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers the default adapter type for objects which implement
        /// <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HttpActionContext"/>.
        /// </summary>
        public static void RegisterDefaultValidatableObjectAdapter(Type adapterType)
        {
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            DefaultValidatableFactory = (metadata, context) => (ModelValidator)constructor.Invoke(new object[] { metadata, context });
        }

        /// <summary>
        /// Registers the default adapter factory for objects which implement
        /// <see cref="IValidatableObject"/>.
        /// </summary>
        public static void RegisterDefaultValidatableObjectAdapterFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableFactory(factory);

            DefaultValidatableFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetValidatableAdapterConstructor(Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(ModelMetadata), typeof(HttpActionContext) });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", SRResources.DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements, adapterType);
            }

            return constructor;
        }

        private static void ValidateValidatableAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", SRResources.Common_TypeMustDriveFromType, adapterType, typeof(ModelValidator));
            }
        }

        private static void ValidateValidatableModelType(Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (!typeof(IValidatableObject).IsAssignableFrom(modelType))
            {
                throw Error.Argument("modelType", SRResources.Common_TypeMustDriveFromType, modelType, typeof(ModelValidator));
            }
        }

        private static void ValidateValidatableFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }

        #endregion
    }
}
