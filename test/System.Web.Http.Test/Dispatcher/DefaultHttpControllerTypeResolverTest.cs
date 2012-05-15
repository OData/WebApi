// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerTypeResolverTest
    {
        public static TheoryDataSet<Type> ValidControllerTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(ValidController),
                    typeof(VALIDController),
                    typeof(VALIDCONTROLLER),
                };
            }
        }

        public static TheoryDataSet<Type> InvalidControllerTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(InvalidControllerStruct),
                    typeof(ControllerWrapper.InvalidNestedController),
                    typeof(InvalidControllerAbstract),
                    typeof(InvalidControllerWithInconsistentName),
                    typeof(InvalidControllerWithNoBaseType)
                };
            }
        }

        [Fact]
        public void DefaultHttpControllerTypeResolver_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => new DefaultHttpControllerTypeResolver(null), "predicate");
        }

        [Theory]
        [PropertyData("ValidControllerTypes")]
        public void IsControllerType_AcceptsValidControllerTypes(Type validControllerType)
        {
            Assert.True(DefaultHttpControllerTypeResolver.IsControllerType(validControllerType));
        }

        [Theory]
        [PropertyData("InvalidControllerTypes")]
        public void IsControllerType_RejectsInvalidControllerTypes(Type invalidControllerType)
        {
            Assert.False(DefaultHttpControllerTypeResolver.IsControllerType(invalidControllerType));
        }

        [Fact]
        public void GetControllerTypes_ThrowsOnNull()
        {
            DefaultHttpControllerTypeResolver resolver = new DefaultHttpControllerTypeResolver();
            Assert.ThrowsArgumentNull(() => resolver.GetControllerTypes(null), "assembliesResolver");
        }

        [Fact]
        public void GetControllerTypes_FindsTypes()
        {
            // Arrange
            DefaultHttpControllerTypeResolver resolver = new DefaultHttpControllerTypeResolver();
            Mock<IAssembliesResolver> mockAssemblyResolver = new Mock<IAssembliesResolver>();
            mockAssemblyResolver.Setup(a => a.GetAssemblies()).Returns(new List<Assembly> 
            {
                null,
                new MockExportedTypesAssembly(ThrowException.ReflectionTypeLoadException,
                    typeof(InvalidControllerStruct),
                    typeof(ValidController), 
                    typeof(ControllerWrapper.InvalidNestedController),
                    typeof(InvalidControllerWithInconsistentName)),
                new MockExportedTypesAssembly(ThrowException.Exception,
                    typeof(InvalidControllerAbstract),
                    typeof(InvalidControllerWithNoBaseType)),
                new MockExportedTypesAssembly(ThrowException.None,
                    typeof(VALIDController), 
                    typeof(VALIDCONTROLLER), 
                    typeof(InvalidControllerStruct)),
            });

            // Act
            ICollection<Type> actualControllerTypes = resolver.GetControllerTypes(mockAssemblyResolver.Object);

            // Assert
            Assert.Equal(3, actualControllerTypes.Count);
            Assert.True(actualControllerTypes.Contains(typeof(ValidController)));
            Assert.True(actualControllerTypes.Contains(typeof(VALIDController)));
            Assert.True(actualControllerTypes.Contains(typeof(VALIDCONTROLLER)));
        }
    }

    public enum ThrowException
    {
        None = 0,
        Exception,
        ReflectionTypeLoadException,
    }

    public class MockExportedTypesAssembly : Assembly
    {
        private ThrowException _throwException;
        private Type[] _exportedTypes;

        public MockExportedTypesAssembly(ThrowException throwException, params Type[] exportedTypes)
        {
            _throwException = throwException;
            _exportedTypes = exportedTypes;
        }

        public override bool IsDynamic
        {
            get { return false; }
        }

        public override Type[] GetExportedTypes()
        {
            switch (_throwException)
            {
                case ThrowException.Exception:
                    throw new Exception("GetExportedTypes exception");

                case ThrowException.ReflectionTypeLoadException:
                    throw new ReflectionTypeLoadException(_exportedTypes, null, "GetExportedTypes exception");
            }

            return _exportedTypes;
        }
    }

    public class ControllerWrapper
    {
        public class InvalidNestedController : ApiController
        {
        }
    }

    public struct InvalidControllerStruct
    {
    }

    public abstract class InvalidControllerAbstract
    {
    }

    public class InvalidControllerWithInconsistentName
    {
    }

    public class InvalidControllerWithNoBaseType
    {
    }

    public class ValidController : ApiController
    {
    }

    public class VALIDController : ApiController
    {
    }

    public class VALIDCONTROLLER : ApiController
    {
    }
}
