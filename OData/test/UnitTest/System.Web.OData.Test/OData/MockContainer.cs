// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    public class MockContainer : IServiceProvider
    {
        private IServiceProvider _rootContainer;

        public MockContainer(Action<IContainerBuilder> action = null)
        {
            InitializeConfiguration(action);
        }

        public MockContainer(IEdmModel model)
        {
            InitializeConfiguration(b => b.AddService(ServiceLifetime.Singleton, sp => model));
        }

        public HttpConfiguration Configuration { get; private set; }

        public object GetService(Type serviceType)
        {
            if (_rootContainer == null)
            {
                _rootContainer = Configuration.GetODataRootContainer();
            }

            return _rootContainer.GetService(serviceType);
        }

        private void InitializeConfiguration(Action<IContainerBuilder> action)
        {
            Configuration = new HttpConfiguration();
            Configuration.EnableODataDependencyInjectionSupport(action);
        }
    }
}
