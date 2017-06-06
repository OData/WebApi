using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nuwa.Sdk;
using Xunit.Sdk;

namespace Nuwa.WebStack.Descriptor
{
    /// <summary>
    /// TestDescriptor is a translator to convert test class type to useful information.
    /// </summary>
    public class TestTypeDescriptor
    {
        private ITypeInfo _testClassType;
        private IEnumerable<Type> _testControllerTypes;

        /// <summary>
        /// Ctor
        /// </summary>
        public TestTypeDescriptor(ITypeInfo testClassType)
        {
            _testClassType = testClassType;
        }

        /// <summary>
        /// The type information of the test class
        /// </summary>
        public ITypeInfo TestTypeInfo
        {
            get { return _testClassType; }
        }

        /// <summary>
        /// The method marked by <paramref name="NuwaConfigurationAttribute"/>
        /// </summary>
        public MethodInfo ConfigureMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaConfigurationAttribute>();
            }
        }

        /// <summary>
        /// The method marked by <paramref name="NuwaWebConfigAttribute"/>
        /// </summary>
        public MethodInfo WebConfigMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaWebConfigAttribute>();
            }
        }

        /// <summary>
        /// The method marked by <paramref name="NuwaWebDeploymentConfigurationAttribute"/>
        /// </summary>
        public MethodInfo WebDeployConfigMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaWebDeploymentConfigurationAttribute>();
            }
        }

        /// <summary>
        /// The reflect types of the test api controllers this test depends on.
        /// </summary>
        public IEnumerable<Type> TestControllerTypes
        {
            get
            {
                if (_testControllerTypes == null)
                {
                    var types = _testClassType.GetCustomAttributes<NuwaTestControllerAttribute>()
                                                        .Select(one => one.ControllerType);

                    if (types != null && types.Any())
                    {
                        _testControllerTypes = types.ToList();
                    }
                    else
                    {
                        _testControllerTypes = Enumerable.Empty<Type>();
                    }
                }

                return _testControllerTypes;
            }
        }

        /// <summary>
        /// The assemly this test class belongs to
        /// </summary>
        public Assembly TestAssembly
        {
            get
            {
                return this.TestTypeInfo.Type.Assembly;
            }
        }

        /// <summary>
        /// Get the reflect method info based on it's attribute
        /// </summary>
        public MethodInfo GetDesignatedMethod<T>() where T : Attribute
        {
            var markedMethod = this.TestTypeInfo.GetMethodMarkedByAttribute(typeof(T));

            if (!markedMethod.Any())
            {
                return null;
            }
            else if (markedMethod.Length == 1)
            {
                return markedMethod.First().MethodInfo;
            }
            else
            {
                throw new InvalidOperationException(
                    "There are more than one methods are marked by attribute " + typeof(T).Name + ".");
            }
        }
    }
}