// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class BuildManagerViewEngineTest
    {
        [Fact]
        public void BuildManagerProperty()
        {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager(expectedVirtualPath: null, compiledType: null);

            // Act
            engine.BuildManager = buildManagerMock;

            // Assert
            Assert.Same(engine.BuildManager, buildManagerMock);
        }

        [Fact]
        public void FileExistsReturnsTrueForExistingPath()
        {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager("some path", typeof(object));
            engine.BuildManager = buildManagerMock;

            // Act
            bool result = engine.FileExists("some path");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void FileExistsReturnsFalseWhenBuildManagerFileExistsReturnsFalse()
        {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager("some path", false);
            engine.BuildManager = buildManagerMock;

            // Act
            bool result = engine.FileExists("some path");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ViewPageActivatorConsultsSetActivatorResolver()
        {
            // Arrange
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>();

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(activator.Object);

            //Assert
            Assert.Equal(activator.Object, engine.ViewPageActivator);
        }

        [Fact]
        public void ViewPageActivatorDelegatesToActivatorResolver()
        {
            // Arrange
            var activator = new Mock<IViewPageActivator>();
            var activatorResolver = new Resolver<IViewPageActivator> { Current = activator.Object };

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(activatorResolver: activatorResolver);

            // Assert
            Assert.Equal(activator.Object, engine.ViewPageActivator);
        }

        [Fact]
        public void ViewPageActivatorDelegatesToDependencyResolverWhenActivatorResolverIsNull()
        {
            // Arrange
            var viewInstance = new object();
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(object));
            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            dependencyResolver.Setup(dr => dr.GetService(typeof(object))).Returns(viewInstance).Verifiable();

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(dependencyResolver: dependencyResolver.Object);
            engine.ViewPageActivator.Create(controllerContext, typeof(object));

            // Assert
            dependencyResolver.Verify();
        }

        [Fact]
        public void ViewPageActivatorDelegatesToActivatorCreateInstanceWhenDependencyResolverReturnsNull()
        {
            // Arrange
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(NoParameterlessCtor));
            var dependencyResolver = new Mock<IDependencyResolver>();

            var engine = new TestableBuildManagerViewEngine(dependencyResolver: dependencyResolver.Object);

            // Act
            MissingMethodException ex = Assert.Throws<MissingMethodException>( // Depend on the fact that Activator.CreateInstance cannot create an object without a parameterless ctor
                () => engine.ViewPageActivator.Create(controllerContext, typeof(NoParameterlessCtor))
                );

            // Assert           
            Assert.Contains("System.Activator.CreateInstance(", ex.StackTrace);
        }

        [Fact]
        public void ActivatorResolverAndDependencyResolverAreNeverCalledWhenViewPageActivatorIsPassedInContstructor()
        {
            // Arrange
            var controllerContext = new ControllerContext();
            var expectedController = new Goodcontroller();

            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>();

            var resolverActivator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            var activatorResolver = new Resolver<IViewPageActivator> { Current = resolverActivator.Object };

            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);

            //Act
            var engine = new TestableBuildManagerViewEngine(activator.Object, activatorResolver, dependencyResolver.Object);

            //Assert
            Assert.Same(activator.Object, engine.ViewPageActivator);
        }

        private class NoParameterlessCtor
        {
            public NoParameterlessCtor(int x)
            {
            }
        }

        private class TestableBuildManagerViewEngine : BuildManagerViewEngine
        {
            public TestableBuildManagerViewEngine()
                : base()
            {
            }

            public TestableBuildManagerViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator)
            {
            }

            public TestableBuildManagerViewEngine(IViewPageActivator viewPageActivator = null, IResolver<IViewPageActivator> activatorResolver = null, IDependencyResolver dependencyResolver = null)
                : base(viewPageActivator, activatorResolver, dependencyResolver)
            {
            }

            public new IViewPageActivator ViewPageActivator
            {
                get { return base.ViewPageActivator; }
            }

            protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
            {
                throw new NotImplementedException();
            }

            protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
            {
                throw new NotImplementedException();
            }

            public bool FileExists(string virtualPath)
            {
                return base.FileExists(null, virtualPath);
            }
        }
    }
}
